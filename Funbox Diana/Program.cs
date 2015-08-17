using System;
using LeagueSharp;
using LeagueSharp.Common;
namespace Diana
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
          if (ObjectManager.Player.ChampionName != "Diana")
            return;
          _q = new Spell(SpellSlot.Q, 830);
          _w = new Spell(SpellSlot.W, 200);
          _e = new Spell(SpellSlot.E, 350);
          _r = new Spell(SpellSlot.R, 825);
          _q.SetSkillshot(0.25f, 70, 1200, false, SkillshotType.SkillshotCircle);
          _config = new Menu("Diana", "Diana", true);
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
          if (_r.Level <= 2)
            {
              ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
          var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Magical);
          if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
              if (target != null)
                {
                  if (_q.IsReady() && target.IsValidTarget(_q.Range))
                    {
                      var QPred = _q.GetPrediction(target);
                      if (QPred.Hitchance >= HitChance.High)
                        {
                          _q.Cast(QPred.CastPosition);
                        }
                    }
                  if (_w.IsReady() && target.IsValidTarget(_w.Range))
                    {
                      _w.Cast();
                    }
                  if (_e.IsReady() && target.IsValidTarget(_e.Range))
                    {
                      _e.Cast();
                    }
                  if (target.HasBuff("dianamoonlight"))
                    {
                      if (_r.IsReady() && target.IsValidTarget(_r.Range))
                        {
                          _r.CastOnUnit(target);
                        }
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
          if (spell.Name.ToLower().Contains("dianaarc") || spell.Name.ToLower().Contains("dianaorbs") || spell.Name.ToLower().Contains("dianavortex") || spell.Name.ToLower().Contains("dianateleport"))
            {
              Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
            }
        }
    }
}