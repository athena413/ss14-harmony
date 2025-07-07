using Content.Server.Stunnable;
using Content.Shared._Goobstation.Bingle.Components;
using Content.Shared._Goobstation.Bingle.EntitySystems;
using Content.Shared.Destructible;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Bingle.EntitySystems;

public sealed class BinglePitSystem : SharedBinglePitSystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly StunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BinglePitComponent, DestructionEventArgs>(OnDestruction);
    }

    // The container system was complaining in shared so no prediction for thou
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BinglePitFallingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Fell ||
                _gameTiming.CurTime < comp.InsertIntoPitTime)
                continue;

            if (!comp.Pit.Valid ||
                !Exists(comp.Pit) ||
                !TryComp<BinglePitComponent>(comp.Pit, out var pit))
            {
                RemCompDeferred<BinglePitFallingComponent>(uid); // Pit doesn't exist anymore, remove the falling comp.
                continue;
            }

            _containerSystem.Insert(uid, pit.Pit);

            comp.Fell = true;
            Dirty(uid, comp);

            _stunSystem.TryKnockdown(uid, TimeSpan.FromSeconds(1), true); // Make them fall to the ground and drop anything they have.
        }
    }

    private void OnDestruction(Entity<BinglePitComponent> entity, ref DestructionEventArgs args)
    {
        foreach (var uid in _containerSystem.EmptyContainer(entity.Comp.Pit))
        {
            RemComp<BinglePitFallingComponent>(uid);
            _stunSystem.TryKnockdown(uid, entity.Comp.FallIntoPitTime, false);
        }

        var query = EntityQueryEnumerator<BinglePitFallingComponent>();
        while (query.MoveNext(out var fallingUid, out var fallingComp))
        {
            RemCompDeferred(fallingUid, fallingComp);
        }
    }

    protected override void PlayFallingAudio(Entity<BinglePitComponent> entity)
    {
        _audioSystem.PlayPvs(entity.Comp.FallingSound, entity);
    }
}
