using RBot;
using RBot.Items;
using RBot.Monsters;
using RBot.Quests;
using System;
using System.Collections.Generic;

public class Script
{
	public ScriptInterface bot;

	public int QuestID = 2857;

	public List<string> AllowedItems = new List<string>()
	{
		"Escherion's Helm"
	};

	public List<string> BlockedItems = new List<string>()
	{
		"DuckStick2000",
		"Unidentified 2",
		"Unidentified 3",
		"Unidentified 7",
		"Unidentified 8",
		"Unidentified 12",
		"Unidentified 14",
		"Unidentified 15",
		"Unidentified 17",
		"Unidentified 18",
		"Unidentified 30",
		"Unidentified 32",
		"Unidentified 33"
	};

	public void ScriptMain(ScriptInterface instance)
	{
		bot = instance;

		bot.Options.SafeTimings = true;
		bot.Options.RestPackets = true;
		bot.Options.InfiniteRange = true;
		bot.Options.ExitCombatBeforeQuest = true;

		bot.Skills.SkillTimeout = 5;
		bot.Skills.SkillTimer = 2000;

		bot.Skills.Clear();

		bot.Skills.Add(1);
		bot.Skills.Add(2);
		bot.Skills.Add(3);
		bot.Skills.Add(4);

		bot.Skills.StartTimer();

		bot.Player.LoadBank();

		bot.Wait.ForBankLoad();

		SortItems();
		CheckBank();

		bot.Drops.Pickup.Clear();

		AllowedItems.ForEach(itemName => bot.Drops.Add(itemName));

		bot.Drops.Start();

		while (!bot.ShouldExit())
		{
			if (bot.Map.Name != "escherion")
			{
				bot.Player.Join("escherion");
				bot.Wait.ForMapLoad("escherion");
			}

			CheckQuest();

			bot.Sleep(1000);

			HuntEscherion();

			bot.Sleep(2500);
		}

		bot.Skills.Clear();
		bot.Skills.StopTimer();

		bot.Drops.Stop();
	}

	public void HuntEscherion()
	{
		while (!bot.Inventory.Contains("Escherion's Helm"))
		{
			Monster escherion = bot.Monsters.MapMonsters.Find(monster => monster.Name == "Escherion");
			Monster staffOfInversion = bot.Monsters.MapMonsters.Find(monster => monster.Name == "Staff of Inversion");

			if (bot.Player.Cell != escherion.Cell)
				bot.Player.Jump(escherion.Cell, "Left");

			if (staffOfInversion.Alive)
				bot.Player.Attack(staffOfInversion.Name);
			else
				bot.Player.Attack(escherion.Name);
		}

		bot.Player.Jump(bot.Player.Cell, bot.Player.Pad);
	}

	public void CheckBank()
	{
		bot.Bank.BankItems.ForEach(item => {
			if (AllowedItems.Contains(item.Name))
			{
				if (item.Category == ItemCategory.Item || item.Category == ItemCategory.Resource)
					bot.Bank.ToInventory(item.Name);
				else
				{
					AllowedItems.Remove(item.Name);
					BlockedItems.Add(item.Name);
				}
			}
		});
	}

	public void SortItems()
	{
		bot.Quests.EnsureLoad(QuestID);

		Quest quest = bot.Quests.QuestTree.Find(q => q.ID == QuestID);

		quest.Rewards.ForEach(reward => {
			if (!BlockedItems.Contains(reward.Name) && !AllowedItems.Contains(reward.Name))
				AllowedItems.Add(reward.Name);
		});
	}

	public void CheckQuest()
	{
		if (bot.Quests.CanComplete(QuestID))
		{
			if (bot.Player.InCombat)
			{
				bot.Player.CancelTarget();
				bot.Player.Jump(bot.Player.Cell, bot.Player.Pad);
				bot.Wait.ForCombatExit();
			}

			bot.Quests.EnsureComplete(QuestID);
			bot.Wait.ForQuestComplete(QuestID);
		}

		if (!bot.Quests.ActiveQuests.Exists(quest => quest.ID == QuestID))
		{
			bot.Quests.EnsureAccept(QuestID);
			bot.Wait.ForQuestAccept(QuestID);
		}
	}
}