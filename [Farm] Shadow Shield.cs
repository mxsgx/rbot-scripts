using RBot;
using RBot.Options;
using System.Collections.Generic;

public class Script
{
	public ScriptInterface bot;

	public List<int> QuestIDs = new List<int>()
    {
        3828
    };

    public List<IOption> Options = new List<IOption>()
	{
		new Option<int>("shadowShieldStackNumber", "Shadow Shield Stack Number", "Number of \"Shadow Sield\" item you want to stack. Default value is 500", 500),
		new Option<bool>("withDaily", "With Daily Quest", "With daily quest? True for yes and False for no.", false)
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

		ConfigureScript();

		bot.Drops.Pickup.Clear();

		bot.Drops.Add("Shadow Shield");

		bot.Drops.Start();

		while (!bot.ShouldExit())
		{
			if (!bot.Inventory.Contains("Shadow Shield", bot.Config.Get<int>("shadowShieldStackNumber")))
			{
				if (bot.Map.Name != "lightguardwar")
				{
					bot.Player.Join("lightguardwar");
					bot.Wait.ForMapLoad("lightguardwar");
					bot.Sleep(2000);
				}

				if (bot.Config.Get<bool>("withDaily"))
				{
					CheckQuest();

					bot.Sleep(1500);
				}

				HuntForSomeItems(
					"Sigrid Sunshield",
					new string[] {"Shadow Shield", "Broken Blade"},
					new int[] {bot.Config.Get<int>("shadowShieldStackNumber"), 1},
					new bool[] {false, true}
				);

				bot.Sleep(1500);
			}
		}

		bot.Skills.Clear();
		bot.Skills.StopTimer();

		bot.Drops.Stop();
	}

	public void HuntForSomeItems(string name, string[] items, int[] quantities, bool[] isTempItems)
	{
		while (!HasFulfilledItem(items, quantities, isTempItems))
		{
			bot.Player.Hunt(name);
			bot.Sleep(1000);
		}
	}

	public bool HasFulfilledItem(string[] items, int[] quantities, bool[] isTempItems)
	{
		bool fulfilledItem = false;

		for (int i = 0; i < items.Length; i++)
		{
			string item = items[i];
			int quantity = quantities[i];
			bool isTemp = isTempItems[i];

			fulfilledItem = isTemp ? bot.Inventory.ContainsTempItem(item, quantity) : bot.Inventory.Contains(item, quantity);

			if (fulfilledItem) break;
		}

		return fulfilledItem;
	}

	public void ConfigureScript()
	{
		if (bot.Config.Get<bool>("withDaily") && bot.Player.IsMember)
			QuestIDs.Add(3827);
	}

	public void CheckQuest()
	{
		QuestIDs.ForEach(id => {
			bot.Quests.EnsureLoad(id);

			if (!bot.Quests.IsInProgress(id) && !bot.Quests.IsDailyComplete(id) && bot.Quests.IsAvailable(id))
			{
				bot.Quests.EnsureAccept(id);
				bot.Wait.ForQuestAccept(id);

				bot.Sleep(1000);
			}

			if (bot.Quests.CanComplete(id))
			{
				if (bot.Player.InCombat)
				{
					bot.Player.CancelTarget();
					bot.Player.Jump(bot.Player.Cell, bot.Player.Pad);
					bot.Wait.ForCombatExit();
				}

				bot.Quests.EnsureComplete(id);
				bot.Wait.ForQuestComplete(id);

				bot.Sleep(1000);
			}
		});
	}
}