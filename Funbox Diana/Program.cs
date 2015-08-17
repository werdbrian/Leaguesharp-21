using System;
using LeagueSharp;
using LeagueSharp.Common;
namespace Diana
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
          if (ObjectManager.Player.ChampionName != "Diana")
            return;
          _q = new Spell(SpellSlot.Q, 830);
          _w = new Spell(SpellSlot.W, 200);
          _e = new Spell(SpellSlot.E, 350);
          _r = new Spell(SpellSlot.R, 825);
          _q.SetSkillshot(0.25f, 70, 1200, false, SkillshotType.SkillshotCircle);
          _q.MinHitChance = HitChance.VeryHigh;
          _config = new Menu("Diana", "Diana", true);
          _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
          var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
          TargetSelector.AddToMenu(targetSelectorMenu);
          _config.AddSubMenu(targetSelectorMenu);
          _config.AddToMainMenu();
          Game.OnUpdate += Game_OnUpdate;
        }
      private static void Game_OnUpdate(EventArgs args)
        {
          if (_r.Level == 0 || _r.Level == 1 || _r.Level == 2)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
          var qtarget = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
          var wtarget = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
          var etarget = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
          var rtarget = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
          if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
              if (_q.IsReady())
                {
                  _q.Cast(qtarget);
                }
              if (_w.IsReady())
                {
                  _w.Cast(wtarget);
                }
              if (_e.IsReady())
                {
                  _e.Cast(etarget);
                }
              if (rtarget.HasBuff("dianamoonlight"))
                {
                  if (_r.IsReady())
                    {
                      _r.CastOnUnit(rtarget);
                    }
                }
            }
        }
    }
}