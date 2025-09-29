using Beaversims.Core;
using Beaversims.Core.Common;
using Beaversims.Core.Parser;
using Beaversims.Core.Specs;
using Beaversims.Core.Specs.Paladin.Holy.Abilities;
using System.Diagnostics;
using System.Text.Json;

var totalTime = Stopwatch.StartNew();

var json = File.ReadAllText("fNMDadYmgtWGJ349-90-10.json");
var userId = 10;
using var doc = JsonDocument.Parse(json);
var userEvents = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("userEvents").GetProperty("data");
var playerData = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("playerData").GetProperty("data");
var combatantEvents = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("combatantEvents").GetProperty("data");
var fightData = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("fightData")[0];
var userInfo = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("userEvents").GetProperty("data")[0];

var fight = FightParser.ParseFight(fightData);
var allUnits = UnitParser.ParseUnits(playerData, combatantEvents, userInfo, userId);
var events = EventParser.ParseUserEvents(userEvents, allUnits);

var user = allUnits.GetUser();
user.Spec.SpecIteration(events, allUnits);
var swGains = ProcessEvents.GetStatWeights(events, fight);
TestUtils.PrintStatWeights(swGains);

totalTime.Stop();
Console.WriteLine($"Total Time: {totalTime}");