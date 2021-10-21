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

	/// <summary>
	/// Allowed items list that will pickup.
	/// </summary>
	public List<string> AllowedItems = new List<string>()
	{
		// All items that listed below is initial. Feel free to add as much as you want.
		"Escherion's Helm"
	};

	/// <summary>
	/// Blocked items list. Actually this list is used for filter.
	/// </summary>
	public List<string> BlockedItems = new List<string>()
	{
		// All items that listed below is initial. Feel free to add as much as you want.
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

		// Clear drop pickup list
		bot.Drops.Pickup.Clear();

		// Add all allowed items to drop pickup list
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

	/// <summary>
	/// Hunt Escherion for Escherion's Helm. If Staff of Inversion alive kill first.
	/// </summary>
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

	/// <summary>
	/// Check items in bank. If item category (that listed in quest reward) is "Item" or "Resource" move to inventory
	/// otherwise keep it in the bank and add item to blocked items and remove from allowed items if already exists
	/// so drop grabber won't pickup.
	/// </summary>
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

	/// <summary>
	/// Get reward items and add to allowed items list if item name not in blocked items list.
	/// </summary>
	public void SortItems()
	{
		bot.Quests.EnsureLoad(QuestID);

		Quest quest = bot.Quests.QuestTree.Find(q => q.ID == QuestID);

		quest.Rewards.ForEach(reward => {
			if (!BlockedItems.Contains(reward.Name) && // Not in blocked items
			    !AllowedItems.Contains(reward.Name)    // Not in allowed items (prevent duplicate)
			)
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