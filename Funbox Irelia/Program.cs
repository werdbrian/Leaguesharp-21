using System;
using LeagueSharp;
using LeagueSharp.Common;
namespace Irelia
{
  public class Program
    {
      private static Menu _config;
      private static Orbwalking.Orbwalker _orbwalker;
      private static Spell _q;
      private static Spell _w;
      private static Spell _e;
      private static Spell _r;
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
          else if (_w.Level == 0 || _w.Level == 1 || _w.Level == 2)
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
          var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
          var etarget = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
          var rtarget = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Physical);
          if (_e.IsReady() && (etarget.Health/etarget.MaxHealth)*100 > (ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100)
            {
              _e.CastOnUnit(etarget);
            }
          if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
              if (_q.IsReady() && qtarget.Distance(ObjectManager.Player.Position) < _q.Range)
                {
                  _w.Cast();
                }
              if (_q.IsReady() && qtarget.Distance(ObjectManager.Player.Position) > Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
                {
                  _q.CastOnUnit(qtarget);
                }
              else if (_w.IsReady())
                {
                  _w.Cast();
                }
              else if (_e.IsReady())
                {
                  _e.CastOnUnit(etarget);
                }
              else if (_r.IsReady())
                {
                  _r.Cast(rtarget);
                }
              if (qtarget.IsValidTarget(500))
                {
                  Items.UseItem(3144, qtarget);
                  Items.UseItem(3146, qtarget);
                  Items.UseItem(3153, qtarget);
                }
              if (qtarget.IsValidTarget(350))
                {
                  Items.UseItem(3074);
                  Items.UseItem(3077);
                  Items.UseItem(3142);
                  Items.UseItem(3143);
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