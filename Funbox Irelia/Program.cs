using System;
using LeagueSharp;
using LeagueSharp.Common;
namespace Irelia
{
  public class Program
    {
      private static Menu _config;
      private static Orbwalking.Orbwalker _orbwalker;
      private static Spell _q, _w, _e, _r;
      private static void Main(string[] args)
        {
          CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
      private static void Game_OnGameLoad(EventArgs args)
        {
          if (ObjectManager.Player.ChampionName != "Irelia")
            return;
          _q = new Spell(SpellSlot.Q, 900);
          _w = new Spell(SpellSlot.W, 250);
          _e = new Spell(SpellSlot.E, 425);
          _r = new Spell(SpellSlot.R, 1000);
          _r.SetSkillshot(0.25f, 70, 1200, false, SkillshotType.SkillshotLine);
          _r.MinHitChance = HitChance.Medium;
          _config = new Menu("Irelia", "Irelia", true);
          _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
          var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
          TargetSelector.AddToMenu(targetSelectorMenu);
          _config.AddSubMenu(targetSelectorMenu);
          _config.AddToMainMenu();
          Obj_AI_Base.OnProcessSpellCast += oncast;
          Game.OnUpdate += Game_OnUpdate;
        }
      private static void Game_OnUpdate(EventArgs args)
        {
          if (_q.Level == 0) 
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
            }
          else if (_e.Level == 0)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
          else if (_w.Level <= 2)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
            }
          if (_r.Level == 0)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
          else if (_w.Level == 3)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
            }
          else if (_e.Level == 1)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
          if (_w.Level == 4)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
            }
          else if (_e.Level == 2)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
          else if (_r.Level == 1)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
          if ((_e.Level == 3 || _e.Level == 4) && _r.Level == 2)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
          else if (_q.Level == 1 || _q.Level == 2)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
            }
          else if (_r.Level == 2)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
          if (_q.Level == 3 || _q.Level == 4 && _r.Level == 3)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
            }
          var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
          if (target != null)
            {
              if (_e.IsReady() && target.IsValidTarget(_e.Range) && (target.Health/target.MaxHealth)*100 > (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100)
                {
                  _e.CastOnUnit(target);
                }
            }
          if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
              if (target != null)
                {
                  if (_q.IsReady() && _w.IsReady() && target.IsValidTarget(_q.Range))
                    {
                      _w.Cast();
                    }
                  if (_q.IsReady() && target.IsValidTarget(_q.Range) && target.Distance(ObjectManager.Player.Position) > Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
                    {
                      _q.CastOnUnit(target);
                    }
                  else if (_w.IsReady() && target.IsValidTarget(_w.Range))
                    {
                      _w.Cast();
                    }
                  else if (_e.IsReady() && target.IsValidTarget(_e.Range))
                    {
                      _e.CastOnUnit(target);
                    }
                  else if (_r.IsReady() && target.IsValidTarget(_r.Range))
                    {
                      _r.Cast(target);
                    }
                  if (target.IsValidTarget(500))
                    {
                      Items.UseItem(3144, target);
                      Items.UseItem(3146, target);
                      Items.UseItem(3153, target);
                    }
                  if (target.IsValidTarget(350))
                    {
                      Items.UseItem(3074);
                      Items.UseItem(3077);
                      Items.UseItem(3142);
                      Items.UseItem(3143);
                    }
                }
            }
        }
      private static void oncast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
          var spell = args.SData;
          if (!sender.IsMe)
            {
              return;
            }
          if (spell.Name.ToLower().Contains("ireliagatotsu") || spell.Name.ToLower().Contains("ireliahitenstyle") || spell.Name.ToLower().Contains("ireliaequilibriumstrike") || spell.Name.ToLower().Contains("ireliatranscendentblades"))
            {
              Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
            }
        }
    }
}