using Content.Server.Power.Components;
using Content.Server.PowerCell;
using JetBrains.Annotations;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.Containers;

namespace Content.Server.Power.EntitySystems;

[UsedImplicitly]
internal sealed class HandChargerSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<HandChargerComponent, ItemSlotsComponent>();


        while (query.MoveNext(out var uid, out var charger, out var itemSlots))
        {
            if (!_container.TryGetContainingContainer(uid, out var containerComp))
                return;

            if (!EntityManager.TryGetComponent(containerComp.Owner, out HandsComponent? hands))
                return;

            var batteryToCharge = new List<BatteryComponent>();

            foreach (var tempHand in hands.Hands.Values)
            {
                if (tempHand.HeldEntity == null) continue;

                if (!TryComp(tempHand.HeldEntity.Value, out BatteryComponent? heldBattery))
                    continue;

                if (heldBattery.CurrentCharge == heldBattery.MaxCharge) continue;

                batteryToCharge.Add(heldBattery);
            }

            var batteryToDischarge = GetFirstBattery(uid, charger, itemSlots);

            if (batteryToDischarge == null)
                return;

            TransferPower(uid, batteryToDischarge, batteryToCharge, charger, frameTime);
        }
    }

    private BatteryComponent? GetFirstBattery(EntityUid uid, HandChargerComponent charger, ItemSlotsComponent itemSlots)
    {
        for (var i = 0; i < 6; ++i)
        {
            var tempEntity = _itemSlots.GetItemOrNull(uid, $"charger_slot_{i}", itemSlots);
            if (tempEntity == null) continue;

            if (!TryComp(tempEntity, out BatteryComponent? batteryComponent)) {
                if (!_powerCell.TryGetBatteryFromSlot(tempEntity.Value, out batteryComponent)) return null;
            }

            return batteryComponent.CurrentCharge > 0 ? batteryComponent : null;
        }

        return null;
    }

    private void TransferPower(EntityUid uid, BatteryComponent batteryToDischarge, List<BatteryComponent> batteryToCharge, HandChargerComponent component, float frameTime)
    {
        float deltaChargeRate = component.ChargeRate * frameTime;

        float chargeRate = deltaChargeRate / batteryToCharge.Count;

        if (batteryToDischarge.CurrentCharge < deltaChargeRate)
        {
            chargeRate = batteryToDischarge.CurrentCharge / batteryToCharge.Count * frameTime;
            batteryToDischarge.CurrentCharge = 0;
        }
        else
            batteryToDischarge.CurrentCharge -= deltaChargeRate;

        foreach (var tempBattery in batteryToCharge)
        {
            tempBattery.CurrentCharge += chargeRate;
            // Just so the sprite won't be set to 99.99999% visibility
            if (tempBattery.MaxCharge - tempBattery.CurrentCharge < 0.01 || tempBattery.CurrentCharge > tempBattery.MaxCharge)
            {
                tempBattery.CurrentCharge = tempBattery.MaxCharge;
            }
        }
    }
}
