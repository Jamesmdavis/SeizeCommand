using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SeizeCommand.Interactions.Interactors;
using SeizeCommand.Attack;
using SeizeCommand.Weapon;
using SeizeCommand.Aiming;

namespace SeizeCommand.Interactions.Interactables
{
    public class GunSeat : AbstractNetworkSeat
    {
        [SerializeField] private GameObject weapon;
        [SerializeField] private GameObject weaponSlot;

        protected override void TakeSeat(Interactor interactor)
        {
            base.TakeSeat(interactor);

            AttackManager attackManager = interactor.Player.GetComponent<AttackManager>();
            Gun gun = weapon.GetComponent<Gun>();

            attackManager.CurrentWeapon = gun;
            gun.Sender = interactor.Player;

            MouseAim aim = weaponSlot.GetComponent<MouseAim>();
            aim.enabled = true;
        }

        protected override void LeaveSeat(Interactor interactor)
        {
            base.LeaveSeat(interactor);

            AttackManager playerAttack = interactor.Player.GetComponent<AttackManager>();
            playerAttack.DefaultWeapon();

            MouseAim aim = weaponSlot.GetComponent<MouseAim>();
            aim.enabled = false;
        }
    }
}