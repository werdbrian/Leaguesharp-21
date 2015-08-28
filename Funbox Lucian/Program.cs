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
        private static Items.Item _botrk = new Items.Item(3153, 550f), _cutlass = new Items.Item(3144, 550f), _youmuus = new Items.Item(3142, 550f);
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
            _w.MinHitChance = HitChance.Medium;
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
            _config.SubMenu("Combo").SubMenu("Items").SubMenu("Botrk").AddItem(new MenuItem("Botrk", "Botrk").SetValue(true));
            _config.SubMenu("Combo").SubMenu("Items").SubMenu("Cutlass").AddItem(new MenuItem("Cutlass", "Cutlass").SetValue(true));
            _config.SubMenu("Combo").SubMenu("Items").SubMenu("Youmuus").AddItem(new MenuItem("Youmuus", "Youmuus").SetValue(true));
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
                if (target != null && target.IsValidTarget(1150))
                {
                    if (_q.IsReady())
                    {
                        //Q Extended logic
                        if (target.Distance(ObjectManager.Player) > 675)
                        {
                            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 675, MinionTypes.All, MinionTeam.NotAlly);
                            foreach (var minion in minions)
                            {
                                if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                                {
                                    if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manh)
                                    {
                                        //laneclear, mixed, lasthit certain champions + check mana
                                        if (cerh && _q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, 1150), 0, HitChance.VeryHigh))
                                        {
                                            _q2.CastOnUnit(minion);
                                        }
                                        //laneclear, mixed, lasthit all champions + check mana
                                        if (!cerha && _q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, 1150), 0, HitChance.VeryHigh))
                                        {
                                            _q2.CastOnUnit(minion);
                                        }
                                    }
                                }
                                if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                                {
                                    if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > manc)
                                    {
                                        //combo certain champions + check mana
                                        if (useqex && cerc && _q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, 1150), 0, HitChance.VeryHigh))
                                        {
                                            _q2.CastOnUnit(minion);
                                        }
                                        //combo all champions + check mana
                                        if (useqex && !cerco && _q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, 1150), 0, HitChance.VeryHigh))
                                        {
                                            _q2.CastOnUnit(minion);
                                        }
                                    }
                                }
                                //Q Extended killsteal
                                if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) > target.Health && qkill && _q2.WillHit(target, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, 1150), 0, HitChance.VeryHigh))
                                {
                                    _q2.CastOnUnit(minion);
                                }
                            }
                        }
                        //Q killsteal
                        if (target.Distance(ObjectManager.Player) <= 675)
                        {
                            if (qkill && ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) > target.Health)
                            {
                                _q.CastOnUnit(target);
                            }
                        }
                    }
                    //W killsteal
                    if (_w.IsReady() && target.Distance(ObjectManager.Player) <= 1000)
                    {
                        if (wkill && ObjectManager.Player.GetSpellDamage(target, SpellSlot.W) > target.Health)
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
        }
        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var enemy = TargetSelector.GetTarget(700, TargetSelector.DamageType.Physical);
            var obj = (Obj_AI_Base) target;
            var quse = _config.Item("qcom").GetValue<bool>();
            var wuse = _config.Item("wcom").GetValue<bool>();
            var botuse = _config.Item("Botrk").GetValue<bool>();
            var cutuse = _config.Item("Cutlass").GetValue<bool>();
            var youuse = _config.Item("Youmuus").GetValue<bool>();
            var EMode =_config.Item("emod").GetValue<StringList>().SelectedIndex;
            var pos = Geometry.CircleCircleIntersection(ObjectManager.Player.ServerPosition.To2D(), Prediction.GetPrediction(obj, 0.25f).UnitPosition.To2D(), 425, Orbwalking.GetRealAutoAttackRange(obj));
            if (enemy != null && enemy.IsValidTarget())
            {
                if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    //Q Usage
                    if (_q.IsReady() && quse)
                    {
                        _q.CastOnUnit(enemy);
                    }
                    //W Usage
                    if (!_q.IsReady() && _w.IsReady() && wuse)
                    {
                        _w.Cast(enemy);
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
                                    _e.Cast(ObjectManager.Player.ServerPosition.Extend(obj.ServerPosition, -425));
                                }
                            }
                            //Q Usage
                            if (!_e.IsReady() && _q.IsReady() && quse)
                            {
                                _q.CastOnUnit(enemy);
                            }
                            //W Usage
                            if (!_q.IsReady() && !_e.IsReady() && _w.IsReady() && wuse)
                            {
                                _w.Cast(enemy);
                            }
                        break;
                        //E To Mouse
                        case 1:
                            //E Usage
                            if (_e.IsReady())
                            {
                                _e.Cast(ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, 425));
                            }
                            //Q Usage
                            if (!_e.IsReady() && _q.IsReady() && quse)
                            {
                                _q.CastOnUnit(enemy);
                            }
                            //W Usage
                            if (!_q.IsReady() && !_e.IsReady() && _w.IsReady() && wuse)
                            {
                                _w.Cast(enemy);
                            }
                        break;
                        //E None
                        case 2:
                            //Q Usage
                            if (_q.IsReady() && quse)
                            {
                                _q.CastOnUnit(enemy);
                            }
                            //W Usage
                            if (!_q.IsReady() && _w.IsReady() && wuse)
                            {
                                _w.Cast(enemy);
                            }
                        break;
                    }
                    if (enemy.Distance(ObjectManager.Player) < 550)
                    {
                        //ITEMS
                        if (_botrk.IsReady() && botuse)
                        {
                            _botrk.Cast(enemy);
                        }
                        if (_cutlass.IsReady() && cutuse)
                        {
                            _cutlass.Cast(enemy);
                        }
                        if (_youmuus.IsReady() && youuse)
                        {
                            _youmuus.Cast();
                        }
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
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 675, Qran.Color);
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
