using System;
using System.Linq;
using LeagueSharp;
using System.Drawing;
using LeagueSharp.Common;
namespace Lucian
{
    public class Program
    {
        private static Menu _config;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _q2, _w, _w2, _e;
        private static int _lastTick;
        private static string[] select = {"Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jinx", "Kalista", "KogMaw", "Lucian", "MissFortune","Quinn","Sivir","Teemo","Tristana","TwistedFate","Twitch","Urgot","Varus","Vayne"};
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Lucian")
            {
                return;
            }
            _q = new Spell(SpellSlot.Q, 675);
            _q2 = new Spell(SpellSlot.Q, 1150);
            _w = new Spell(SpellSlot.W, 700);
            _w2 = new Spell(SpellSlot.W, 1000);
            _e = new Spell(SpellSlot.E, 425);
            _q2.SetSkillshot(0.25f, 70, 3000, false, SkillshotType.SkillshotLine);
            _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
            _w2.SetSkillshot(0.25f, 70, 1500, true, SkillshotType.SkillshotLine);
            _config = new Menu("Lucian", "Lucian", true);
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);
            _config.SubMenu("Combo").SubMenu("Q Extended settings").AddItem(new MenuItem("qexcomhero", "Q Only Certain Champions").SetValue(true));
            foreach (var hero in HeroManager.Enemies) { _config.SubMenu("Combo").SubMenu("Q Extended settings").SubMenu("Certain Champions").AddItem(new MenuItem("autocom" + hero.ChampionName, hero.ChampionName).SetValue(select.Contains(hero.ChampionName))); }
            _config.SubMenu("Combo").SubMenu("Q Extended settings").AddItem(new MenuItem("manac", "Mana").SetValue(new Slider(33, 100, 0)));
            _config.SubMenu("Combo").SubMenu("E settings").AddItem(new MenuItem("emodswitch", "Switch Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Combo").SubMenu("E settings").AddItem(new MenuItem("emod", "E Mode").SetValue(new StringList(new[]{"Safe", "To mouse", "None"})));
            _config.SubMenu("Combo").AddItem(new MenuItem("qcom", "Q").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("qexcom", "Q Extended").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("wcom", "W").SetValue(true));
            _config.SubMenu("Killsteal").AddItem(new MenuItem("qkil", "Q / Q Extended").SetValue(true));
            _config.SubMenu("Killsteal").AddItem(new MenuItem("wkil", "W").SetValue(true));
            _config.SubMenu("Harass").AddItem(new MenuItem("qexharhero", "Q Only Certain Champions").SetValue(true));
            foreach (var hero in HeroManager.Enemies) { _config.SubMenu("Harass").SubMenu("Certain Champions").AddItem(new MenuItem("autohar" + hero.ChampionName, hero.ChampionName).SetValue(select.Contains(hero.ChampionName))); }
            _config.SubMenu("Harass").AddItem(new MenuItem("manah", "Mana").SetValue(new Slider(33, 100, 0)));
            _config.SubMenu("Drawings").SubMenu("Spells").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _config.SubMenu("Drawings").SubMenu("Spells").AddItem(new MenuItem("QExRange", "Q Extended range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _config.SubMenu("Drawings").SubMenu("Spells").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _config.SubMenu("Drawings").SubMenu("Spells").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _config.SubMenu("Drawings").SubMenu("Spells").AddItem(new MenuItem("EaaRange", "E + AA range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _config.SubMenu("Drawings").SubMenu("Spells").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _config.SubMenu("Drawings").AddItem(new MenuItem("emodraw", "E Mode Text").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("tdraw", "Active Enemy").SetValue(new Circle(true, Color.GreenYellow)));
            _config.AddItem(new MenuItem("dels", "Delay before spell").SetValue(new Slider(100, 200, 0)));
            _config.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += oncast;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Game.OnUpdate += Game_OnUpdate;
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            Emode();
            var manh = _config.Item("manah").GetValue<Slider>().Value;
            var manc = _config.Item("manac").GetValue<Slider>().Value;
            var cerha = _config.Item("qexharhero").GetValue<bool>();
            var cerco = _config.Item("qexcomhero").GetValue<bool>();
            var qkill = _config.Item("qkil").GetValue<bool>();
            var wkill = _config.Item("wkil").GetValue<bool>();
            var useqex = _config.Item("qexcom").GetValue<bool>();
            foreach (var target in HeroManager.Enemies)
            {
                var cerh = _config.Item("autohar" + target.ChampionName).GetValue<bool>();
                var cerc = _config.Item("autocom" + target.ChampionName).GetValue<bool>();
                if (target != null)
                {
                    if (_q.IsReady())
                    {
                        //Q Extended logic
                        if (target.IsValidTarget(_q2.Range) && target.Distance(ObjectManager.Player) > _q.Range)
                        {
                            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
                            foreach (var minion in minions)
                            {
                                //laneclear, mixed, lasthit certain champions + check mana
                                if (cerh && (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manh && (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit))
                                {
                                    if (_q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                                    {
                                        _q2.CastOnUnit(minion);
                                    }
                                }
                                //laneclear, mixed, lasthit all champions + check mana
                                if (!cerha && (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manh && (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit))
                                {
                                    if (_q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                                    {
                                        _q2.CastOnUnit(minion);
                                    }
                                }
                                //combo certain champions + check mana
                                if (useqex && cerc && (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manc && _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                                {
                                    if (_q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                                    {
                                        _q2.CastOnUnit(minion);
                                    }
                                }
                                //combo all champions + check mana
                                if (useqex && !cerco && (ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manc && _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                                {
                                    if (_q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                                    {
                                        _q2.CastOnUnit(minion);
                                    }
                                }
                                //Q Extended killsteal
                                if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && qkill)
                                {
                                    if (_q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                                    {
                                        _q2.CastOnUnit(minion);
                                    }
                                }
                            }
                        }
                        //Q killsteal
                        if (qkill && target.IsValidTarget(_q.Range) && ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) > target.Health)
                        {
                            _q.CastOnUnit(target);
                        }
                    }
                    //W killsteal
                    if (wkill && _w.IsReady() && target.IsValidTarget(_w2.Range) && ObjectManager.Player.GetSpellDamage(target, SpellSlot.W) > target.Health)
                    {
                        var WPred = _w2.GetPrediction(target);
                        if (WPred.Hitchance >= HitChance.High)
                        {
                            _w2.Cast(WPred.CastPosition);
                        }
                    }
                }
            }
        }
        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            var obj = (Obj_AI_Base) target;
            var dely = _config.Item("dels").GetValue<Slider>().Value;
            var quse = _config.Item("qcom").GetValue<bool>();
            var wuse = _config.Item("wcom").GetValue<bool>();
            var EMode =_config.Item("emod").GetValue<StringList>().SelectedIndex;
            var pos = Geometry.CircleCircleIntersection(ObjectManager.Player.ServerPosition.To2D(), Prediction.GetPrediction(obj, 0.25f).UnitPosition.To2D(), _e.Range, Orbwalking.GetRealAutoAttackRange(obj));
            if (t != null && t.IsValidTarget())
            {
                if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    //Q Usage
                    if (_q.IsReady() && quse)
                    {
                        _q.CastOnUnit(t);
                    }
                    //W Usage
                    if (!_q.IsReady() && _w.IsReady() && wuse)
                    {
                        Utility.DelayAction.Add(dely, Wuse);
                    }
                }
                if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    switch (EMode)
                    {
                        //E Safe
                        case 0:
                            //E Usage
                            if (_e.IsReady())
                            {
                                if (pos.Count() > 0)
                                {
                                    _e.Cast(pos.MinOrDefault(i => i.Distance(Game.CursorPos)));
                                }
                                else
                                {
                                    _e.Cast(ObjectManager.Player.ServerPosition.Extend(obj.ServerPosition, -_e.Range));
                                }
                            }
                            //Q Usage
                            if (!_e.IsReady() && _q.IsReady() && quse)
                            {
                                Utility.DelayAction.Add(dely, Quse);
                            }
                            //W Usage
                            if (!_q.IsReady() && !_e.IsReady() && _w.IsReady() && wuse)
                            {
                                Utility.DelayAction.Add(dely, Wuse);
                            }
                        break;
                        //E To Mouse
                        case 1:
                            //E Usage
                            if (_e.IsReady())
                            {
                                _e.Cast(ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, _e.Range));
                            }
                            //Q Usage
                            if (!_e.IsReady() && _q.IsReady() && quse)
                            {
                                Utility.DelayAction.Add(dely, Quse);
                            }
                            //W Usage
                            if (!_q.IsReady() && !_e.IsReady() && _w.IsReady() && wuse)
                            {
                                Utility.DelayAction.Add(dely, Wuse);
                            }
                        break;
                        //E None
                        case 2:
                            //Q Usage
                            if (_q.IsReady() && quse)
                            {
                                _q.CastOnUnit(t);
                            }
                            //W Usage
                            if (!_q.IsReady() && _w.IsReady() && wuse)
                            {
                                Utility.DelayAction.Add(dely, Wuse);
                            }
                        break;
                    }
                    //BOTRK
                    if (t.Distance(ObjectManager.Player) < 550)
                    {
                        Items.UseItem(3144, t);
                        Items.UseItem(3153, t);
                    }
                }
            }
        }
        //Reset Auto Attack After Spells
        private static void oncast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
            {
                return;
            }
            if (spell.Name.ToLower().Contains("lucianq") || spell.Name.ToLower().Contains("lucianw") || spell.Name.ToLower().Contains("luciane"))
            {
                Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
            }
        }
        //E Mode Switch
        private static void Emode()
        {
            var lasttime = Environment.TickCount - _lastTick;
            var EMode = _config.Item("emod").GetValue<StringList>().SelectedIndex;
            if (!_config.Item("emodswitch").GetValue<KeyBind>().Active || lasttime <= Game.Ping)
            {
                return;
            }
            switch (EMode)
            {
                case 0:
                    _config.Item("emod").SetValue(new StringList(new[]{"Safe", "To mouse", "None"}, 1));
                    _lastTick = Environment.TickCount + 300;
                break;
                case 1:
                    _config.Item("emod").SetValue(new StringList(new[]{"Safe", "To mouse", "None"}, 2));
                    _lastTick = Environment.TickCount + 300;
                break;
                case 2:
                    _config.Item("emod").SetValue(new StringList(new[]{"Safe", "To mouse", "None"}));
                    _lastTick = Environment.TickCount + 300;
                break;
            }
        }
        //Q usage
        private static void Quse()
        {
            var t = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            _q.CastOnUnit(t);
        }
        //W usage
        private static void Wuse()
        {
            var t = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            var WPred = _w.GetPrediction(t);
            if (WPred.Hitchance >= HitChance.Low)
            {
                _w.Cast(WPred.CastPosition);
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
            var drt = _config.Item("tdraw").GetValue<Circle>();
            var emdraw = _config.Item("emodraw").GetValue<bool>();
            var EMode =_config.Item("emod").GetValue<StringList>().SelectedIndex;
            var Qran = _config.Item("QRange").GetValue<Circle>();
            var Qexran = _config.Item("QExRange").GetValue<Circle>();
            var Wran = _config.Item("WRange").GetValue<Circle>();
            var Eran = _config.Item("ERange").GetValue<Circle>();
            var Eaaran = _config.Item("EaaRange").GetValue<Circle>();
            var Rran = _config.Item("RRange").GetValue<Circle>();
            //Draw Target
            if (drt.Active)
            {
                var td = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
                if (td != null && td.IsValidTarget())
                {
                    Render.Circle.DrawCircle(td.Position, 115f, drt.Color, 1);
                }
            }
            //Draw E Mode
            if (emdraw)
            {
                switch (EMode)
                {
                    case 0:
                        Drawing.DrawText(wts[0], wts[1], Color.White, "Safe");
                    break;
                    case 1:
                        Drawing.DrawText(wts[0], wts[1], Color.White, "To mouse");
                    break;
                    case 2:
                        Drawing.DrawText(wts[0], wts[1], Color.White, "None");
                    break;
                }
            }
            //Spells Range
            if (Qran.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, Qran.Color);
            }
            if (Qexran.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _q2.Range, Qexran.Color);
            }
            if (Wran.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _w2.Range, Wran.Color);
            }
            if (Eran.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, Eran.Color);
            }
            if (Eaaran.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range + Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), Eaaran.Color);
            }
            if (Rran.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1400, Rran.Color);
            }
        }
    }
}
