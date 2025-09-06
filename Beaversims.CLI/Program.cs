using Beaversims.Core;
using Beaversims.Core.Parser;
using System.Text.Json;



var json = File.ReadAllText("fNMDadYmgtWGJ349-90-10.json");
var userId = 10;
using var doc = JsonDocument.Parse(json);
var userEvents = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("userEvents").GetProperty("data");
var playerData = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("playerData").GetProperty("data");
var combadantEvents = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("combatantEvents").GetProperty("data");
var fightData = doc.RootElement.GetProperty("data").GetProperty("reportData").GetProperty("report").GetProperty("fightData")[0];

var fight = FightParser.ParseFight(fightData);
var allUnits = UnitParser.ParseUnits(playerData, combadantEvents, userId);
var events = EventParser.ParseUserEvents(userEvents);

