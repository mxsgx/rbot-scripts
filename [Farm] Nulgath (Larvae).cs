using RBot;
using RBot.Items;
using RBot.Monsters;
using RBot.Quests;
using RBot.Options;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class Script
{
	public ScriptInterface bot;

	public int QuestID = 2566;

	/// <summary>
	/// Allowed items list that will pickup.
	/// </summary>
	public List<string> AllowedItems = new List<string>()
	{
		// All items that listed below is initial. Feel free to add as much as you want.
		"Mana Energy for Nulgath"
	};

	/// <summary>
	/// Blocked items list. Actually this list is used for filter.
	/// </summary>
	public List<string> BlockedItems = new List<string>()
	{
		// All items that listed below is initial. Feel free to add as much as you want.
		"Hex Blade of Nulgath",
		"Voidfangs of Nulgath",
		"Bane of Nulgath",
		"Shifter Helm of Nulgath",
		"Nulgath Horns"
	};

	public string OptionsStorage = "[Farm] Nulgath (Larvae)";

	public List<IOption> Options = new List<IOption>()
	{
		new Option<int>(
			"manaEnergyForNulgathStackNumber",
			"Mana Energy for Nulgath Stack Number",
			"Prioritize to get \"Mana Energy for Nulgath\" item until specified stack number before turn in.",
			1
		)
	};

	public void ScriptMain(ScriptInterface instance)
	{
		bot = instance;

		bot.Options.SafeTimings = true;
		bot.Options.RestPackets = true;
		bot.Options.InfiniteRange = true;
		bot.Options.ExitCombatBeforeQuest = true;

		SortItems();

		if (!ValidateConfiguration())
			return;

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

		CheckBank();

		// Clear drop pickup list
		bot.Drops.Pickup.Clear();

		// Add all allowed items to drop pickup list
		AllowedItems.ForEach(itemName => bot.Drops.Add(itemName));

		bot.Drops.Start();

		// Accept quest for the first time
		DoQuest(true);

		while (!bot.ShouldExit())
		{
			if (bot.Map.Name != "elemental")
			{
				bot.Player.Join("elemental");
				bot.Wait.ForMapLoad("elemental");
			}

			bot.Player.Jump("r5", "Down");
			bot.Player.HuntForItem("Mana Golem", "Mana Energy for Nulgath", bot.Config.Get<int>("manaEnergyForNulgathStackNumber"), false, bot.Drops.RejectElse);

			bot.Sleep(1000);

			CheckQuest();
		}

		bot.Skills.Clear();
		bot.Skills.StopTimer();

		bot.Drops.Stop();
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
	/// Validate configuration from user input.
	/// </summary>
	public bool ValidateConfiguration()
	{
		int manaEnergyForNulgathStackNumber = bot.Config.Get<int>("manaEnergyForNulgathStackNumber");

		Quest quest = bot.Quests.QuestTree.Find(q => q.ID == QuestID);
		ItemBase item = quest.Requirements.Find(i => i.Name == "Mana Energy for Nulgath");

		bool passed = (manaEnergyForNulgathStackNumber > 0 && manaEnergyForNulgathStackNumber <= item.MaxStack);

		if (!passed)
			MessageBox.Show($"Mana Energy for Nulgath Stack number must be 1 to {item.MaxStack}.", "Configuration Error!");

		return passed;
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
		while (bot.Inventory.Contains("Mana Energy for Nulgath"))
		{
			bot.Player.HuntForItem("Mana Imp|Mana Falcon", "Charged Mana Energy for Nulgath", 5, true, bot.Drops.RejectElse);

			bot.Sleep(2500);

			DoQuest();
		}
	}

	public void DoQuest(bool firstTime = false)
	{
		if (bot.Quests.CanComplete(QuestID) && !firstTime)
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