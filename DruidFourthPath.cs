using MelonLoader;
using BTD_Mod_Helper;
using DruidFourthPath;
using PathsPlusPlus;
using Il2CppAssets.Scripts.Models.Towers;
using BTD_Mod_Helper.Api.Enums;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppSystem.IO;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Models.Towers.Filters;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models;
using System;
using BTD_Mod_Helper.Api.Towers;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Utils;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using BTD_Mod_Helper.Api.Display;
using System.Linq;
using BTD_Mod_Helper.Api;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Harmony;
using Il2CppAssets.Scripts.Models.Towers.Weapons;

[assembly: MelonInfo(typeof(DruidFourthPath.DruidFourthPathMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace DruidFourthPath;

public class DruidFourthPathMod : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        
    }

    public override void OnGameModelLoaded(GameModel model)
    {
        base.OnGameModelLoaded(model);
    }

    public class DruidFourthPath : PathPlusPlus
    {
        public override string Tower => TowerType.Druid;

        public override int UpgradeCount => 5;
    }
    public class ScorchedThorns : UpgradePlusPlus<DruidFourthPath>
    {
        public override int Cost => 450;
        public override int Tier => 1;
        public override string Icon => VanillaSprites.HardThornsUpgradeIcon;
        public override string Description => "Thorns can now pop lead balloons.";
        public override string? Portrait => "ScorchedThorns";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var damageModel in towerModel.GetDescendants<DamageModel>().ToArray())
            {
                damageModel.immuneBloonProperties &= ~BloonProperties.Lead;
            }
            if (IsHighestUpgrade(towerModel))
            {
                towerModel.display = new PrefabReference("26128ff71a76aa54d872488e967dbc2a");
            }
        }
    }
    public class HeartOfFlames : UpgradePlusPlus<DruidFourthPath>
    {
        public override int Cost => 1000;
        public override int Tier => 2;
        public override string Icon => VanillaSprites.FireballUpgradeIcon;
        public override string Description => "Attacks burn Bloons.";
        public override string? Portrait => "HeartOfFlames";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            var attackModel = towerModel.GetAttackModel();
            var weaponModel = attackModel.weapons[0];
            var projectileModel = weaponModel.projectile;
            var damageModel = projectileModel.GetDamageModel();
            projectileModel.AddBehavior(Game.instance.model.GetTower(TowerType.MortarMonkey, 0, 0, 2).GetDescendant<AddBehaviorToBloonModel>().Duplicate());
            projectileModel.GetBehavior<AddBehaviorToBloonModel>().filters = null;
            if (IsHighestUpgrade(towerModel))
            {
                towerModel.display = new PrefabReference("7d5ae1d99aa38fc46a38ac199546be8f");
            }
        }
    }
    public class SmallPhoenixDisplay : ModDisplay
    {
        public override PrefabReference BaseDisplayReference => new PrefabReference("1e5aa5cc44941da43a90880b50d5d112");
        public override float Scale => 0.25f;
    }

    public class SmallFireDisplay : ModDisplay
    {
        public override PrefabReference BaseDisplayReference => new PrefabReference("01dfdf7fe33be28409a9c2e1db9bbec0");
        public override float Scale => 0.75f;
    }


    public class DruidOfTheFire : UpgradePlusPlus<DruidFourthPath>
    {
        public override int Cost => 2800;
        public override int Tier => 3;
        public override string Icon => VanillaSprites.SummonPhoenixUpgradeIcon;
        public override string Description => "The Druid summons a small Phoenix that follows the Bloons throughout the whole map.";
        public override string? Portrait => "DruidOfFire";
        public override void ApplyUpgrade(TowerModel towerModel)
        {
            var attackModel = towerModel.GetAttackModel();
            var weaponModel = attackModel.weapons[0];
            var projectileModel = weaponModel.projectile;
            var damageModel = projectileModel.GetDamageModel();
            towerModel.AddBehavior(Game.instance.model.GetTower(TowerType.Etienne).GetBehavior<DroneSupportModel>().Duplicate());
            var airModel = towerModel.GetBehavior<DroneSupportModel>();
            airModel.droneModel.GetBehavior<AirUnitModel>().display = ModContent.CreatePrefabReference<SmallPhoenixDisplay>();
            airModel.droneModel.GetAttackModel().weapons[0].Rate = 0.15f;
            airModel.droneModel.GetAttackModel().weapons[0].projectile.ApplyDisplay<SmallFireDisplay>();

            /*
             * Game produces errors if this is run, lights on back of Phoenix are required.
             * Game does not crash but errors are ugly.
            */

            /*airModel.droneModel.GetBehavior<DroneIdleModel>().blueLight = null;
            airModel.droneModel.GetBehavior<DroneIdleModel>().redLight = null;
            airModel.droneModel.GetBehavior<DroneIdleModel>().greenLight = null;
            airModel.droneModel.GetBehavior<DroneIdleModel>().purpleLight = null;
            airModel.droneModel.GetBehavior<DroneIdleModel>().yellowLight = null;*/
            
            airModel.droneModel.range = towerModel.range * 2;
            airModel.droneModel.GetAttackModel().range = towerModel.range * 2;
            airModel.droneModel.GetAttackModel().GetBehavior<PursuitSettingCustomModel>().mustBeInRangeOfParent = false;
            FilterModel[] filterModel = { new FilterInvisibleModel("FilterInvisibleModel_", true, false) };
            airModel.droneModel.GetAttackModel().GetBehavior<AttackFilterModel>().filters = new Il2CppReferenceArray<FilterModel>(filterModel);
            airModel.droneModel.GetAttackModel().weapons[0].projectile.AddBehavior(Game.instance.model.GetTower(TowerType.MortarMonkey, 0, 0, 2).GetDescendant<AddBehaviorToBloonModel>().Duplicate());
            airModel.droneModel.GetAttackModel().weapons[0].projectile.GetBehavior<AddBehaviorToBloonModel>().filters = null;
            int[] collPass = { -1, 0 };
            airModel.droneModel.GetAttackModel().weapons[0].projectile.collisionPasses = new Il2CppStructArray<int>(collPass);
            airModel.droneModel.GetAttackModel().weapons[0].projectile.GetDamageModel().immuneBloonProperties = BloonProperties.Purple;
            airModel.droneModel.GetAttackModel().weapons[0].ejectY += 5;
            if (IsHighestUpgrade(towerModel))
            {
                towerModel.display = new PrefabReference("91a57f974e6096246af86ecb62db0cac");
            }
        }
        public class MediumPhoenixDisplay : ModDisplay
        {
            public override PrefabReference BaseDisplayReference => new PrefabReference("1e5aa5cc44941da43a90880b50d5d112");
            public override float Scale => 0.5f;
        }

        public class MediumFireDisplay : ModDisplay
        {
            public override PrefabReference BaseDisplayReference => new PrefabReference("01dfdf7fe33be28409a9c2e1db9bbec0");
            public override float Scale => 1.0f;
        }
        public class MassiveFireDisplay : ModDisplay
        {
            public override PrefabReference BaseDisplayReference => new PrefabReference("01dfdf7fe33be28409a9c2e1db9bbec0");
            public override float Scale => 2.5f;
        }
        public class FireBreath : UpgradePlusPlus<DruidFourthPath>
        {
            public override int Cost => 10000;
            public override int Tier => 4;
            public override string Icon => VanillaSprites.WallOfFireUpgradeIcon;
            public override string Description => "Breathes out a firey breath that eradicates Bloons. Also increases the power of the Phoenix.";
            public override string? Portrait => "FireBreath";

            public override void ApplyUpgrade(TowerModel towerModel)
            {
                var attackModel = towerModel.GetAttackModel();
                var weaponModel = attackModel.weapons[0];
                var projectileModel = weaponModel.projectile;
                var damageModel = projectileModel.GetDamageModel();
                var airModel = towerModel.GetBehavior<DroneSupportModel>();

                var newWep = weaponModel.Duplicate();
                newWep.emission = new EmissionOverTimeModel("EmissionOverTimeModel_", 6, 0.05f, null);
                newWep.projectile.ApplyDisplay<MassiveFireDisplay>();
                newWep.projectile.GetBehavior<TravelStraitModel>().Speed /= 5;
                newWep.projectile.GetBehavior<TravelStraitModel>().Lifespan *= 10;
                newWep.Rate *= 4;
                newWep.projectile.pierce = 25;
                newWep.projectile.GetDamageModel().damage = 15;
                newWep.name = "WeaponModel_FireBreath_";
                attackModel.AddWeapon(newWep);

                airModel.droneModel.GetBehavior<AirUnitModel>().display = ModContent.CreatePrefabReference<MediumPhoenixDisplay>();
                airModel.droneModel.GetAttackModel().weapons[0].projectile.ApplyDisplay<MediumFireDisplay>();
                airModel.droneModel.GetAttackModel().weapons[0].Rate = 0.09f;
                airModel.droneModel.GetAttackModel().weapons[0].projectile.pierce = 7;
                airModel.droneModel.GetAttackModel().weapons[0].projectile.GetDamageModel().damage = 3;
                airModel.droneModel.GetAttackModel().weapons[0].ejectY += 5;
                if (IsHighestUpgrade(towerModel))
                {
                    towerModel.display = new PrefabReference("3e2d4193260639a468f835cfe7488885");
                }
            }
        }
        public class LargePhoenixDisplay : ModDisplay
        {
            public override PrefabReference BaseDisplayReference => new PrefabReference("1e5aa5cc44941da43a90880b50d5d112");
            public override float Scale => 1f;
        }

        public class LargeFireDisplay : ModDisplay
        {
            public override PrefabReference BaseDisplayReference => new PrefabReference("01dfdf7fe33be28409a9c2e1db9bbec0");
            public override float Scale => 1.25f;
        }
        public class ControlledBurning : UpgradePlusPlus<DruidFourthPath>
        {
            public override int Cost => 65000;
            public override int Tier => 5;
            public override string Icon => VanillaSprites.RingOfFireUpgradeIcon;
            public override string Description => "Permantly damages Bloons around itself without harming any other Monkeys.";
            public override string? Portrait => "ControlledBurning";

            public override void ApplyUpgrade(TowerModel towerModel)
            {
                var attackModel = towerModel.GetAttackModel();
                var weaponModel = attackModel.weapons[0];
                var projectileModel = weaponModel.projectile;
                var damageModel = projectileModel.GetDamageModel();
                var airModel = towerModel.GetBehavior<DroneSupportModel>();

                towerModel.AddBehavior(Game.instance.model.GetTower(TowerType.TackShooter, 5).behaviors.FirstOrDefault(model => model.name == "DisplayModel_PassiveRadialHeatPulse").Duplicate());

                var newAtk = Game.instance.model.GetTower(TowerType.TackShooter, 5).GetAttackModel().Duplicate();
                newAtk.RemoveBehavior<CreateEffectWhileAttackingModel>();
                newAtk.weapons[0].Rate = 0.05f;
                newAtk.weapons[0].projectile.pierce = 99999;
                newAtk.weapons[0].projectile.GetDamageModel().damage = 5;
                newAtk.weapons[0].animateOnMainAttack = false;
                newAtk.weapons[0].animation = 0;
                newAtk.weapons[0].animationOffset = 0.0f;
                newAtk.name = "AttackModel_ControlledBurning_";
                towerModel.AddBehavior(newAtk);

                airModel.droneModel.GetBehavior<AirUnitModel>().display = ModContent.CreatePrefabReference<LargePhoenixDisplay>();
                airModel.droneModel.GetAttackModel().weapons[0].projectile.ApplyDisplay<LargeFireDisplay>();
                airModel.droneModel.GetAttackModel().weapons[0].Rate = 0.03f;
                airModel.droneModel.GetAttackModel().weapons[0].projectile.pierce = 15;
                airModel.droneModel.GetAttackModel().weapons[0].projectile.GetDamageModel().damage = 8;
                airModel.droneModel.GetAttackModel().weapons[0].ejectY += 10;

                var breathWep = towerModel.GetDescendants<WeaponModel>().FirstOrDefault(model => model.name == "WeaponModel_FireBreath_");
                breathWep.emission.Cast<EmissionOverTimeModel>().count = 30;
                breathWep.projectile.GetDamageModel().damage *= 1.5f;


                /*airModel.droneModel.GetBehavior<AirUnitModel>().display = ModContent.CreatePrefabReference<MediumPhoenixDisplay>();
                airModel.droneModel.GetAttackModel().weapons[0].projectile.ApplyDisplay<MediumFireDisplay>();
                airModel.droneModel.GetAttackModel().weapons[0].Rate = 0.07f;
                airModel.droneModel.GetAttackModel().weapons[0].projectile.pierce = 7;
                airModel.droneModel.GetAttackModel().weapons[0].projectile.GetDamageModel().damage = 4;*/
                if (IsHighestUpgrade(towerModel))
                {
                    towerModel.display = new PrefabReference("acf836014419c134787007ed2a6304b5");
                }
            }
        }
    }
}