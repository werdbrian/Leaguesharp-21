using System;
using System.Linq;
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
          _config.AddItem(new MenuItem("qrr", "QR to minion R killsteal").SetValue(true));
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
          if (_config.Item("qrr").GetValue<bool>())
            {
              QRkillsteal();
              QRRkillsteal();
              QRRWkillsteal();
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
          if (spell.Name.ToLower().Contains("dianaarc") || spell.Name.ToLower().Contains("dianavortex") || spell.Name.ToLower().Contains("dianateleport"))
            {
              Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
            }
        }
      private static void QRkillsteal()
        {
          var tr = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);
          if (_q.IsReady() && _w.IsReady() && _r.IsReady())
            {
              if (tr.Distance(ObjectManager.Player) < _r.Range && (ObjectManager.Player.GetSpellDamage(tr, SpellSlot.R) + ObjectManager.Player.GetSpellDamage(tr, SpellSlot.W) + ObjectManager.Player.GetSpellDamage(tr, SpellSlot.Q)) > tr.Health)
                {
                  var QPred = _q.GetPrediction(tr);
                  if (QPred.Hitchance >= HitChance.Medium)
                    {
                      _q.Cast(QPred.CastPosition);
                    }
                  if (tr.HasBuff("dianamoonlight"))
                    {
                      _r.CastOnUnit(tr);
                    }
                  if (!_r.IsReady() && _w.IsReady())
                    {
                      _w.Cast();
                    }
                }
            }
          if (!_q.IsReady() && _w.IsReady() && _r.IsReady())
            {
              if (tr.Distance(ObjectManager.Player) < _r.Range && (ObjectManager.Player.GetSpellDamage(tr, SpellSlot.R) + ObjectManager.Player.GetSpellDamage(tr, SpellSlot.W)) > tr.Health)
                {
                  _r.CastOnUnit(tr);
                }
              if (!_r.IsReady())
                {
                  _w.Cast();
                }
            }
          if (!_q.IsReady() && !_w.IsReady() && _r.IsReady())
            {
              if (tr.Distance(ObjectManager.Player) < _r.Range && ObjectManager.Player.GetSpellDamage(tr, SpellSlot.R) > tr.Health)
                {
                  _r.CastOnUnit(tr);
                }
            }
        }        
      private static void QRRkillsteal()
        {
          if (_q.IsReady() && _r.IsReady())
            {
              var tr = TargetSelector.GetTarget(1600, TargetSelector.DamageType.Magical);
              if (tr.Distance(ObjectManager.Player) > 1100 && (ObjectManager.Player.GetSpellDamage(tr, SpellSlot.R)) > tr.Health)
                {
                  var minionQR = ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.IsValidTarget())
                    .FirstOrDefault(x => x.Distance(TargetSelector.GetTarget(_r.Range * 5, TargetSelector.DamageType.Magical)) < _r.Range);
                  if (minionQR != null)
                    {
                      var QPred = _q.GetPrediction(minionQR);
                      if (QPred.Hitchance >= HitChance.Medium)
                        {
                          _q.Cast(QPred.CastPosition);
                        }
                      if (minionQR.HasBuff("dianamoonlight"))
                        {
                          _r.CastOnUnit(minionQR);
                        }
                    }
                }
            }
        }
      private static void QRRWkillsteal()
        {
          if (_q.IsReady() && _w.IsReady() && _r.IsReady())
            {
              var tr = TargetSelector.GetTarget(1600, TargetSelector.DamageType.Magical);
              if (tr.Distance(ObjectManager.Player) > 1100 && (ObjectManager.Player.GetSpellDamage(tr, SpellSlot.R) + ObjectManager.Player.GetSpellDamage(tr, SpellSlot.W)) > tr.Health)
                {
                  var minionQR = ObjectManager.Get<Obj_AI_Minion>()
                    .Where(x => x.IsValidTarget())
                    .FirstOrDefault(x => x.Distance(TargetSelector.GetTarget(_r.Range * 5, TargetSelector.DamageType.Magical)) < _r.Range);
                  if (minionQR != null)
                    {
                      var QPred = _q.GetPrediction(minionQR);
                      if (QPred.Hitchance >= HitChance.Medium)
                        {
                          _q.Cast(QPred.CastPosition);
                        }
                      if (minionQR.HasBuff("dianamoonlight"))
                        {
                          _r.CastOnUnit(minionQR);
                        }
                    }
                }
            }
        }
    }
}