using System.Numerics;
using System.Runtime.CompilerServices;
using Content.Shared._Goobstation.Bingle.Components;
using Content.Shared.Emoting;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Dependency = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Shared._Goobstation.Bingle.EntitySystems;

public abstract class SharedBinglePitSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BinglePitComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BinglePitComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<BinglePitComponent, StepTriggeredOffEvent>(OnStepTriggered);

        #region Prevent BinglePitFalling from interacting

        SubscribeLocalEvent<BinglePitFallingComponent, UseAttemptEvent>(OnFallingAttempt);
        SubscribeLocalEvent<BinglePitFallingComponent, InteractionAttemptEvent>(OnFallingInteract);
        SubscribeLocalEvent<BinglePitFallingComponent, EmoteAttemptEvent>(OnFallingAttempt);
        SubscribeLocalEvent<BinglePitFallingComponent, DropAttemptEvent>(OnFallingAttempt);
        SubscribeLocalEvent<BinglePitFallingComponent, PickupAttemptEvent>(OnFallingAttempt);

        #endregion
    }

    private void OnMapInit(Entity<BinglePitComponent> entity, ref MapInitEvent args)
    {
        entity.Comp.Pit = _containerSystem.EnsureContainer<Container>(entity, BinglePitComponent.PitContainerName);
    }

    private void OnStepTriggerAttempt(Entity<BinglePitComponent> entity, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnStepTriggered(Entity<BinglePitComponent> entity, ref StepTriggeredOffEvent args)
    {
        // don't accept anyone that is already falling.
        if (HasComp<BinglePitFallingComponent>(args.Tripper))
            return;

        var currentLevel = GetCurrentLevel(entity);

        // don't allow anyone alive if the pit is too low.
        if (currentLevel.CanEatLiving == false &&
            HasComp<MobStateComponent>(args.Tripper))
            return;

        // allow only dead bingles in the pit.
        if (HasComp<BingleComponent>(args.Tripper) &&
            TryComp<MobStateComponent>(args.Tripper, out var mobState) &&
            mobState.CurrentState != MobState.Dead)
            return;

        StartFalling(entity.AsNullable(), args.Tripper);
    }

    #region Prevent falling from interacting

    private void OnFallingInteract(Entity<BinglePitFallingComponent> entity, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnFallingAttempt(EntityUid entity, BinglePitFallingComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    #endregion

    private void AddPoints(Entity<BinglePitComponent> entity, int points)
    {
        entity.Comp.Points += points;
        entity.Comp.BingleSpawnPoints += points;

        var currentLevel = GetCurrentLevel(entity);

        while (entity.Comp.BingleSpawnPoints >= currentLevel.PointsPerBingle)
        {
            SpawnBingle(entity);
            entity.Comp.BingleSpawnPoints -= currentLevel.PointsPerBingle;
        }

        UpdateLevel(entity);

        Dirty(entity);
    }

    private void UpdateLevel(Entity<BinglePitComponent> entity)
    {
        if (entity.Comp.CurrentLevel + 1 >= entity.Comp.Levels.Count)
            return;

        var nextLevel = entity.Comp.Levels[entity.Comp.CurrentLevel + 1];

        if (entity.Comp.Points < nextLevel.PointsRequired)
            return;

        entity.Comp.Points -= nextLevel.PointsRequired;

        entity.Comp.CurrentLevel++;

        Dirty(entity);

        _appearanceSystem.SetData(entity, ScaleVisuals.Scale, Vector2.One * nextLevel.Size);
    }

    private void SpawnBingle(Entity<BinglePitComponent> entity)
    {
        var bingle = PredictedSpawnAtPosition(entity.Comp.GhostRoleToSpawn, Transform(entity).Coordinates);

        var bingleSpawner = EnsureComp<BingleSpawnerComponent>(bingle);
        bingleSpawner.Pit = entity;
        Dirty(bingle, bingleSpawner);
    }

    [PublicAPI]
    public void StartFalling(Entity<BinglePitComponent?> entity, EntityUid tripper)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        var extraPoints = entity.Comp.PointsPerObject;

        if (TryComp<MobStateComponent>(tripper, out var mobState) &&
            mobState.CurrentState is MobState.Alive or MobState.Critical)
            extraPoints += entity.Comp.AliveBonus;

        AddPoints((entity, entity.Comp), extraPoints);

        Dirty(entity);

        var falling = EnsureComp<BinglePitFallingComponent>(tripper);
        falling.Pit = entity;
        falling.InsertIntoPitTime = _gameTiming.CurTime + entity.Comp.FallIntoPitTime;
        Dirty(tripper, falling);

        _stunSystem.TryKnockdown(tripper, entity.Comp.FallIntoPitTime, true);

        // PLay falling audio only on server because using the predicted function plays it twice ??
        PlayFallingAudio((entity, entity.Comp));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BinglePitLevel GetCurrentLevel(Entity<BinglePitComponent> entity)
    {
        return entity.Comp.Levels[entity.Comp.CurrentLevel];
    }

    protected virtual void PlayFallingAudio(Entity<BinglePitComponent> entity)
    {
    }
}
