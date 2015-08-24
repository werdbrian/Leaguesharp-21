using System;
using System.Linq;
using LeagueSharp;
using System.Drawing;
using LeagueSharp.Common;
namespace Rengar
{
  public class Program
    {
      private static Menu _config;
      private static Orbwalking.Orbwalker _orbwalker;
      private static Spell _q, _w, _e;
      private static int _lastTick;
      private static void Main(string[] args)
        {
          CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
      private static void Game_OnGameLoad(EventArgs args)
        {
          if (ObjectManager.Player.ChampionName != "Rengar")
            return;
          _q = new Spell(SpellSlot.Q, 230);
          _w = new Spell(SpellSlot.W, 450);
          _e = new Spell(SpellSlot.E, 1000);
          _e.SetSkillshot(0.25f, 70, 1500, true, SkillshotType.SkillshotLine);
          _config = new Menu("Rengar", "Rengar", true);
          _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
          var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
          TargetSelector.AddToMenu(targetSelectorMenu);
          _config.AddSubMenu(targetSelectorMenu);
          _config.AddItem(new MenuItem("ComboMode", "Combo Mode").SetValue(new StringList(new[]{"Empowered Q", "Empowered E"})));
          _config.AddItem(new MenuItem("ComboSwitch", "Combo switch Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
          _config.AddItem(new MenuItem("autoheal", "%hp autoheal").SetValue(new Slider(33, 100, 0)));
          _config.AddItem(new MenuItem("drawtar", "Active Enemy").SetValue(new Circle(true, Color.GreenYellow)));
          _config.AddToMainMenu();
          Drawing.OnDraw += Drawing_OnDraw;
          Obj_AI_Base.OnProcessSpellCast += oncast;
          Game.OnUpdate += Game_OnUpdate;
        }
      private static void oncast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
          var spell = args.SData;
          if (!sender.IsMe)
            {
              return;
            }
          if (spell.Name.ToLower().Contains("rengarq"))
            {
              Orbwalking.ResetAutoAttackTimer();
            }
        }
      private static void Game_OnUpdate(EventArgs args)
        {
          ComboModeSwitch();
          if (ObjectManager.Player.Mana == 5)
            {
              if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 <= _config.Item("autoheal").GetValue<Slider>().Value)
                {
                  _w.Cast();
                }
            }
          foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(2000)))
            {
              if (target != null)
                {
                  if (TargetSelector.GetPriority(target) == 2.5f)
                    {
                      if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
                        {
                          TargetSelector.SetTarget(target);
                          _config.Item("ForceFocusSelected").SetValue(true);
                        }
                    }
                  if (_config.Item("ForceFocusSelected").GetValue<bool>())
                    {
                      if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                        {
                          _config.Item("ForceFocusSelected").SetValue(false);
                        }
                    }
                }
            }
          if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
              var magnet = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
              var qwtarget = TargetSelector.GetTarget(450, TargetSelector.DamageType.Physical);
              var etarget = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
              if (magnet != null)
                {
                  if (magnet.IsValidTarget(_q.Range))
                    {
                      if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                        {
                          if (ObjectManager.Player.Level < 6)
                            {
                              if (_q.IsReady() || _w.IsReady() || _e.IsReady())
                                {
                                  ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, magnet);
                                }
                            }
                          if (ObjectManager.Player.Level >= 6)
                            {
                              ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, magnet);
                            }
                        }
                    }
                }
              if (ObjectManager.Player.Mana <= 4)
                {
                  if (etarget != null)
                    {
                      if (etarget.IsValidTarget(1000))
                        {
                          if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
                            {
                              if (_q.IsReady())
                                {
                                  _q.Cast();
                                }
                            }
                          if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                            {
                              if (_e.IsReady())
                                {
                                  var EPred = _e.GetPrediction(etarget);
                                  if (EPred.Hitchance >= HitChance.Medium)
                                    {
                                      _e.Cast(EPred.CastPosition);
                                    }
                                }
                            }
                        }
                    }
                  if (qwtarget != null)
                    {
                      if (_q.IsReady())
                        {
                          if (qwtarget.IsValidTarget(_q.Range))
                            {
                              _q.Cast();
                            }
                        }
                      if (_w.IsReady())
                        {
                          if (qwtarget.IsValidTarget(_w.Range))
                            {
                              _w.Cast();
                            }
                        }
                      if (_e.IsReady())
                        {
                          if (qwtarget.IsValidTarget(300))
                            {
                              var EPred = _e.GetPrediction(etarget);
                              if (EPred.Hitchance >= HitChance.Medium)
                                {
                                  _e.Cast(EPred.CastPosition);
                                }
                            }
                        }
                    }
                }
              if (ObjectManager.Player.Mana == 5)
                {
                  if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > _config.Item("autoheal").GetValue<Slider>().Value)
                    {
                      switch (_config.Item("ComboMode").GetValue<StringList>().SelectedIndex)
                        {
                          case 0:
                            if (etarget != null)
                              {
                                if (etarget.IsValidTarget(1000))
                                  {
                                    if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
                                      {
                                        if (_q.IsReady())
                                          {
                                            _q.Cast();
                                          }
                                      }
                                    if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                                      {
                                        if (etarget.Distance(ObjectManager.Player.Position) > _q.Range)
                                          {
                                            if (_e.IsReady())
                                              {
                                                var EPred = _e.GetPrediction(etarget);
                                                if (EPred.Hitchance >= HitChance.Medium)
                                                  {
                                                    _e.Cast(EPred.CastPosition);
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                            if (qwtarget != null)
                              {
                                if (_q.IsReady())
                                  {
                                    if (qwtarget.IsValidTarget(_q.Range))
                                      {
                                        _q.Cast();
                                      }
                                  }
                              }
                          break;
                          case 1:
                            if (etarget != null)
                              {
                                if (etarget.IsValidTarget(1000))
                                  {
                                    if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                                      {
                                        if (etarget.Distance(ObjectManager.Player.Position) > _q.Range)
                                          {
                                            if (_e.IsReady())
                                              {
                                                var EPred = _e.GetPrediction(etarget);
                                                if (EPred.Hitchance >= HitChance.Medium)
                                                  {
                                                    _e.Cast(EPred.CastPosition);
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                            if (qwtarget != null)
                              {
                                if (_e.IsReady())
                                  {
                                    if (qwtarget.IsValidTarget(300))
                                      {
                                        var EPred = _e.GetPrediction(etarget);
                                        if (EPred.Hitchance >= HitChance.Medium)
                                          {
                                            _e.Cast(EPred.CastPosition);
                                          }
                                      }
                                  }
                              }
                          break;
                        }
                    }
                }
            }
        }
      private static void Drawing_OnDraw(EventArgs args)
        {
          var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
          var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
          if (_config.Item("drawtar").GetValue<Circle>().Active)
            {
              if (target.IsValidTarget(1000))
                {
                  Render.Circle.DrawCircle(target.Position, 115f, _config.Item("drawtar").GetValue<Circle>().Color, 1);
                }
            }
            switch (_config.Item("ComboMode").GetValue<StringList>().SelectedIndex)
              {
                case 0:
                  Drawing.DrawText(wts[0], wts[1], Color.White, "Q");
                break;
                case 1:
                  Drawing.DrawText(wts[0], wts[1], Color.White, "E");
                break;
              }
         }
       private static void ComboModeSwitch()
          {
            var lasttime = Environment.TickCount - _lastTick;
            if (!_config.Item("ComboSwitch").GetValue<KeyBind>().Active || lasttime <= Game.Ping)
              {
                return;
              }
            switch (_config.Item("ComboMode").GetValue<StringList>().SelectedIndex)
              {
                case 0:
                  _config.Item("ComboMode").SetValue(new StringList(new[]{"Empowered Q", "Empowered E"}, 1));
                  _lastTick = Environment.TickCount + 300;
                break;
                case 1:
                  _config.Item("ComboMode").SetValue(new StringList(new[]{"Empowered Q", "Empowered E"}));
                  _lastTick = Environment.TickCount + 300;
                break;
              }
          }
      }
}