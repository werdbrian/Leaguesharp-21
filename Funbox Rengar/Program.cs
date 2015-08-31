using System;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
namespace Rengar
{
public class Program
{
#region
private static Menu _config;
private static Orbwalking.Orbwalker _orbwalker;
private static Spell _q = new Spell(SpellSlot.Q);
private static Spell _w = new Spell(SpellSlot.W);
private static Spell _e = new Spell(SpellSlot.E);
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
    _e.SetSkillshot(0.25f, 70, 1500, true, SkillshotType.SkillshotLine);
    _e.MinHitChance = HitChance.Medium;
    _config = new Menu("Rengar", "Rengar", true);
    _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
    var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
    TargetSelector.AddToMenu(targetSelectorMenu);
    _config.AddSubMenu(targetSelectorMenu);
    _config.AddItem(new MenuItem("ah", "Auto Heal").SetValue(new Slider(33, 100, 0)));
    _config.AddItem(new MenuItem("eq", "E in Q Mode").SetValue(true));
    _config.AddToMainMenu();
    Game.OnUpdate += Game_OnUpdate;
    Obj_AI_Base.OnProcessSpellCast += oncast;
}
#endregion
#region
private static void Game_OnUpdate(EventArgs args)
{
    var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
    var targett = TargetSelector.GetTarget(350, TargetSelector.DamageType.Physical);
    if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
    {
        if (target.IsValidTarget(1500))
        {
            if (TargetSelector.GetPriority(target) == 2.5f)
            {
                TargetSelector.SetTarget(target);
                if (!_config.Item("ForceFocusSelected").GetValue<bool>())
                {
                    _config.Item("ForceFocusSelected").SetValue(true);
                }
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
    if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
    {
        if (ObjectManager.Player.Mana < 5)
        {
            if (targett.IsValidTarget(350))
            {
                if (targett.Distance(ObjectManager.Player.Position) < 230)
                {
                    if (_q.IsReady())
                    {
                        _q.Cast();
                    }
                }
                if (!ObjectManager.Player.HasBuff("rengarqbase") && !ObjectManager.Player.HasBuff("rengarqemp"))
                {
                    if (_w.IsReady())
                    {
                        _w.Cast();
                    }
                    if (_e.IsReady())
                    {
                        _e.Cast(target);
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
                            _e.Cast(target);
                        }
                    }
                }
            }
        }
        else
        {
            if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > _config.Item("ah").GetValue<Slider>().Value)
            {
                if (targett.IsValidTarget(350))
                {
                    if (ObjectManager.Player.GetSpellDamage(targett, SpellSlot.W) >= targett.Health)
                    {
                        _w.Cast();
                    }
                    else
                    {
                        if (targett.Distance(ObjectManager.Player.Position) < 230)
                        {
                            if (_q.IsReady())
                            {
                                _q.Cast();
                            }
                        }
                    }
                }
                else
                {
                    if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                    {
                        if (target.IsValidTarget(950))
                        {
                            if (target.Distance(ObjectManager.Player.Position) > 300)
                            {
                                if (_e.IsReady())
                                {
                                    _e.Cast(target);
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
}
}
