using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
namespace Lucian
  {
    public class Program
      {
        private static Menu _config;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _q2, _w;
        private static string[] select = {"Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jinx", "Kalista", "KogMaw", "Lucian", "MissFortune","Quinn","Sivir","Teemo","Tristana","TwistedFate","Twitch","Urgot","Varus","Vayne"};
        private static void Main(string[] args)
          {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
          }
        private static void Game_OnGameLoad(EventArgs args)
          {
            if (ObjectManager.Player.ChampionName != "Lucian")
              return;
            _q = new Spell(SpellSlot.Q, 675);
            _q2 = new Spell(SpellSlot.Q, 1200);
            _w = new Spell(SpellSlot.W, 1000);
            _q2.SetSkillshot(0.25f, 70, 3000, false, SkillshotType.SkillshotLine);
            _w.SetSkillshot(0.25f, 70, 1500, false, SkillshotType.SkillshotLine);
            _config = new Menu("Lucian", "Lucian", true);
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);
            foreach (var hero in HeroManager.Enemies) 
              {
                _config.SubMenu("Harass").AddItem(new MenuItem("auto" + hero.ChampionName, hero.ChampionName).SetValue(select.Contains(hero.ChampionName)));
              }
            _config.SubMenu("Harass").AddItem(new MenuItem("manah", "%mana").SetValue(new Slider(33, 100, 0)));
            _config.AddToMainMenu();
            Obj_AI_Base.OnProcessSpellCast += oncast;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Game.OnUpdate += Game_OnUpdate;
          }
        private static void Game_OnUpdate(EventArgs args)
          {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var targetqe = HeroManager.Enemies.Where(hero => hero.IsValidTarget(_q2.Range)).FirstOrDefault(hero => _config.Item("auto" + hero.ChampionName).GetValue<bool>());
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
              {
                if (targetqe != null)
                  {
                    if ((ObjectManager.Player.Mana/ObjectManager.Player.MaxMana)*100 > _config.Item("manah").GetValue<Slider>().Value)
                      {
                        foreach (var minion in minions)
                          {
                            if (_q.IsReady() && _q2.WillHit(targetqe, ObjectManager.Player.ServerPosition.Extend(minion.ServerPosition, _q2.Range), 0, HitChance.VeryHigh))
                              {
                                _q2.CastOnUnit(minion);
                              }
                          }
                      }
                  }
              }
          }
        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
          {
            var ta = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Magical);
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
              {
                if (ta != null)
                  {
                    if (_q.IsReady() && ta.IsValidTarget(_q.Range))
                      {
                        _q.CastOnUnit(ta);
                      }
                    if (!_q.IsReady() && _w.IsReady() && ta.IsValidTarget())
                      {
                        var WPred = _w.GetPrediction(ta);
                        if (WPred.Hitchance >= HitChance.Low)
                          {
                            _w.Cast(WPred.CastPosition);
                          }
                      }
                    if (ta.IsValidTarget(550))
                      {
                        Items.UseItem(3144, ta);
                        Items.UseItem(3153, ta);
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
            if (spell.Name.ToLower().Contains("lucianq") || spell.Name.ToLower().Contains("lucianw") || spell.Name.ToLower().Contains("luciane"))
              {
                Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
              }
          }
      }
  }