using System;
using LeagueSharp;
using System.Drawing;
using LeagueSharp.Common;
namespace Rengar
{
public class Program
{
#region
private static Menu _config;
private static int _lastTick;
private static Orbwalking.Orbwalker _orbwalker;
private static Spell _q = new Spell(SpellSlot.Q, 230);
private static Spell _w = new Spell(SpellSlot.W, 350);
private static Spell _e = new Spell(SpellSlot.E, 950);
#endregion
#region
private static void Main(string[] args)
{
    CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
}
#endregion
#region
private static void Game_OnGameLoad(EventArgs args)
{
    if (ObjectManager.Player.ChampionName != "Rengar")
    {
        return;
    }
    _e.SetSkillshot(0.25f, 90, 1500, true, SkillshotType.SkillshotLine);
    _config = new Menu("Rengar", "Rengar", true);
    _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
    var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
    TargetSelector.AddToMenu(targetSelectorMenu);
    _config.AddSubMenu(targetSelectorMenu);
    _config.SubMenu("Combo Mode").SubMenu("Switch Key").AddItem(new MenuItem("cs", "Combo switch Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
    _config.SubMenu("Combo Mode").AddItem(new MenuItem("em", "E Mode").SetValue(false));
    _config.SubMenu("AutoHeal").AddItem(new MenuItem("ah", "Auto Heal").SetValue(new Slider(33, 100, 0)));
    _config.SubMenu("Q Mode Options").AddItem(new MenuItem("eq", "E in Q Mode").SetValue(true));
    _config.SubMenu("Q Mode Options").AddItem(new MenuItem("aq", "Use WE after Q").SetValue(true));
    _config.SubMenu("LaneClear").AddItem(new MenuItem("ok", "Spells on killable minions").SetValue(true));
    _config.SubMenu("LaneClear").AddItem(new MenuItem("ql", "Q").SetValue(true));
    _config.SubMenu("LaneClear").AddItem(new MenuItem("wl", "W").SetValue(true));
    _config.SubMenu("LaneClear").AddItem(new MenuItem("el", "E").SetValue(true));
    _config.SubMenu("Drawings").AddItem(new MenuItem("cd", "Combo Mode Text").SetValue(true));
    _config.SubMenu("Drawings").AddItem(new MenuItem("dt", "Active Enemy").SetValue(new Circle(true, Color.GreenYellow)));
    _config.AddToMainMenu();
    Game.OnUpdate += Game_OnUpdate;
    Obj_AI_Base.OnProcessSpellCast += oncast;
    Drawing.OnDraw += Drawing_OnDraw;
}
#endregion
#region
private static void Game_OnUpdate(EventArgs args)
{
    Comboswitch();
    Auto();
    switch (_orbwalker.ActiveMode)
    {
        case Orbwalking.OrbwalkingMode.Combo:
            Combo();
        break;
        case Orbwalking.OrbwalkingMode.LaneClear:
            LaneClear();
        break;
    }
}
#endregion
#region
private static void oncast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
{
    var spell = args.SData;
    if (!sender.IsMe)
    {
        return;
    }
    if (spell.Name.ToLower().Contains("rengarq") || spell.Name.ToLower().Contains("rengare"))
    {
        Orbwalking.ResetAutoAttackTimer();
    }
    if (ObjectManager.Player.HasBuff("rengarqbase") || ObjectManager.Player.HasBuff("rengarqemp"))
    {
        Orbwalking.ResetAutoAttackTimer();
    }
}
#endregion
#region
private static void Drawing_OnDraw(EventArgs args)
{
    var t = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
    var tt = TargetSelector.GetTarget(350, TargetSelector.DamageType.Physical);
    if (_config.Item("dt").GetValue<Circle>().Active)
    {
        if (tt.IsValidTarget(350))
        {
            Render.Circle.DrawCircle(tt.Position, 115f, _config.Item("dt").GetValue<Circle>().Color, 1);
        }
        else
        {
            if (t.IsValidTarget(1500))
            {
                Render.Circle.DrawCircle(t.Position, 115f, _config.Item("dt").GetValue<Circle>().Color, 1);
            }
        }
    }
    if (_config.Item("cd").GetValue<bool>())
    {
        if (_config.Item("em").GetValue<bool>())
        {
            Drawing.DrawText(Drawing.WorldToScreen(ObjectManager.Player.Position)[0], Drawing.WorldToScreen(ObjectManager.Player.Position)[1], Color.White, "E");
        }
        else
        {
            Drawing.DrawText(Drawing.WorldToScreen(ObjectManager.Player.Position)[0], Drawing.WorldToScreen(ObjectManager.Player.Position)[1], Color.White, "Q");
        }
    }
}
#endregion
#region
private static void Auto()
{
    if (ObjectManager.Player.Mana == 5)
    {
        if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 <= _config.Item("ah").GetValue<Slider>().Value)
        {
            if (_w.IsReady())
            {
                _w.Cast();
            }
        }
    }
    if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
    {
        var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
        if (TargetSelector.GetPriority(target) == 2.5f)
        {
            TargetSelector.SetTarget(target);
            if (!_config.Item("ForceFocusSelected").GetValue<bool>())
            {
                _config.Item("ForceFocusSelected").SetValue(true);
            }
        }
    }
    else
    {
        if (_config.Item("ForceFocusSelected").GetValue<bool>())
        {
            _config.Item("ForceFocusSelected").SetValue(false);
        }
    }
}
#endregion
#region
private static void Combo()
{
    var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
    var targett = TargetSelector.GetTarget(350, TargetSelector.DamageType.Physical);
    if (ObjectManager.Player.Mana < 5)
    {
        if (targett.IsValidTarget(350))
        {
            if (targett.Distance(ObjectManager.Player.Position) < 230)
            {
                if (_q.IsReady())
                {
                    _q.Cast(targett);
                }
            }
            if (_config.Item("aq").GetValue<bool>())
            {
                if (!ObjectManager.Player.HasBuff("rengarqbase") && !ObjectManager.Player.HasBuff("rengarqemp"))
                {
                    if (_w.IsReady())
                    {
                        _w.Cast(targett);
                    }
                    if (_e.IsReady())
                    {
                        var EPred = _e.GetPrediction(targett);
                        if (EPred.Hitchance >= HitChance.High)
                        {
                            _e.Cast(EPred.CastPosition);
                        }
                    }
                }
            }
            else
            {
                if (_w.IsReady())
                {
                    _w.Cast(targett);
                }
                if (_e.IsReady())
                {
                    var EPred = _e.GetPrediction(targett);
                    if (EPred.Hitchance >= HitChance.High)
                    {
                        _e.Cast(EPred.CastPosition);
                    }
                }
            }
        }
        else
        {
            if (target.IsValidTarget(950))
            {
                if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
                {
                    if (_q.IsReady())
                    {
                        _q.Cast();
                    }
                }
                else
                {
                    if (_e.IsReady())
                    {
                        var EPred = _e.GetPrediction(target);
                        if (EPred.Hitchance >= HitChance.High)
                        {
                            _e.Cast(EPred.CastPosition);
                        }
                    }
                }
            }
        }
    }
    else
    {
        if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > _config.Item("ah").GetValue<Slider>().Value)
        {
            if (_config.Item("em").GetValue<bool>())
            {
                if (targett.IsValidTarget(350))
                {
                    if (_e.IsReady())
                    {
                        var EPred = _e.GetPrediction(targett);
                        if (EPred.Hitchance >= HitChance.High)
                        {
                            _e.Cast(EPred.CastPosition);
                        }
                    }
                }
                else
                {
                    if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                    {
                        if (target.IsValidTarget(950))
                        {
                            if (_e.IsReady())
                            {
                                var EPred = _e.GetPrediction(target);
                                if (EPred.Hitchance >= HitChance.High)
                                {
                                    _e.Cast(EPred.CastPosition);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (targett.IsValidTarget(350))
                {
                    if (targett.Distance(ObjectManager.Player.Position) < 230)
                    {
                        if (_q.IsReady())
                        {
                            _q.Cast(targett);
                        }
                    }
                }
                else
                {
                    if (_config.Item("eq").GetValue<bool>())
                    {
                        if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                        {
                            if (target.IsValidTarget(950))
                            {
                                if (target.Distance(ObjectManager.Player.Position) > 300)
                                {
                                    if (_e.IsReady())
                                    {
                                        var EPred = _e.GetPrediction(target);
                                        if (EPred.Hitchance >= HitChance.High)
                                        {
                                            _e.Cast(EPred.CastPosition);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
#endregion
#region
private static void LaneClear()
{
    if (ObjectManager.Player.Mana < 5)
    {
        var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 350, MinionTypes.All, MinionTeam.NotAlly);
        if (minions != null)
        {
            foreach (var minion in minions)
            {
                if (_config.Item("ok").GetValue<bool>())
                {
                    if (_config.Item("ql").GetValue<bool>())
                    {
                        if (minion.Distance(ObjectManager.Player.Position) < 230)
                        {
                            if (_q.IsReady())
                            {
                                if (ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health)
                                {
                                    _q.Cast(minion);
                                }
                            }
                        }
                    }
                    if (_config.Item("wl").GetValue<bool>())
                    {
                        if (_w.IsReady())
                        {
                            if (ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W) >= minion.Health)
                            {
                                _w.Cast(minion);
                            }
                        }
                    }
                    if (_config.Item("el").GetValue<bool>())
                    {
                        if (_e.IsReady())
                        {
                            if (ObjectManager.Player.GetSpellDamage(minion, SpellSlot.E) >= minion.Health)
                            {
                                var EPred = _e.GetPrediction(minion);
                                if (EPred.Hitchance >= HitChance.High)
                                {
                                    _e.Cast(EPred.CastPosition);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (_config.Item("ql").GetValue<bool>())
                    {
                        if (minion.Distance(ObjectManager.Player.Position) < 230)
                        {
                            if (_q.IsReady())
                            {
                                _q.Cast(minion);
                            }
                        }
                    }
                    if (_config.Item("wl").GetValue<bool>())
                    {
                        if (_w.IsReady())
                        {
                            _w.Cast(minion);
                        }
                    }
                    if (_config.Item("el").GetValue<bool>())
                    {
                        if (_e.IsReady())
                        {
                            var EPred = _e.GetPrediction(minion);
                            if (EPred.Hitchance >= HitChance.High)
                            {
                                _e.Cast(EPred.CastPosition);
                            }
                        }
                    }
                }
            }
        }
    }
}
#endregion
#region
private static void Comboswitch()
{
    var lasttime = Environment.TickCount - _lastTick;
    if (!_config.Item("cs").GetValue<KeyBind>().Active || lasttime <= Game.Ping)
    {
        return;
    }
    if (_config.Item("em").GetValue<bool>())
    {
        _config.Item("em").SetValue(false);
        _lastTick = Environment.TickCount + 300;
    }
    else
    {
        _config.Item("em").SetValue(true);
        _lastTick = Environment.TickCount + 300;
    }
}
#endregion
}
}
