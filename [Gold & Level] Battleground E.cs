using RBot;
using RBot.Options;
using System.Collections.Generic;
using System.Text;

public enum Difficulty
{
	Easy,
	Hard
}

public class Script
{
	public ScriptInterface bot;

	public List<int> QuestIDs = new List<int>()
	{
		3992
	};

	public List<string> QuestItems = new List<string>()
	{
		"Battleground E Opponent Defeated"
	};

	public string OptionsStorage = "[Gold & Level] Battleground E";

	public List<IOption> Options = new List<IOption>()
	{
		new Option<Difficulty>(
			"difficulty",
			"Difficulty",
			"Easy will take \"Level 46-60\" and \"Level 61-75\" quest and \"Hall of Honors\" (Member) as addition quest if Hard difficulty is selected. Default is Easy.",
			Difficulty.Easy
		)
	};

	public StringBuilder Monsters = new StringBuilder("Living Ice");

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

		while (!bot.ShouldExit())
		{
			if (bot.Map.Name != "battlegrounde")
			{
				bot.Player.Join("battlegrounde");
				bot.Wait.ForMapLoad("battlegrounde");
			}

			CheckQuests();

			bot.Sleep(500);

			HuntForSomeItems(Monsters.ToString(), QuestItems.ToArray());

			bot.Sleep(2500);
		}

		bot.Skills.Clear();
		bot.Skills.StopTimer();
	}

	public void ConfigureScript()
	{
		QuestIDs.Add(3991);
		QuestItems.Add("Battleground D Opponent Defeated");

		if (bot.Config.Get<Difficulty>("difficulty") == Difficulty.Hard)
		{
			QuestIDs.Add(3993);
			QuestItems.Add("HonorHall Opponent Defeated");
			Monsters.Append("|Glacial Horror");
		}
		else
			Monsters.Append("|Ice Lord");
	}

	public void HuntForSomeItems(string name, string[] items)
	{
		while (!HasMaxStackItem(items))
		{
			bot.Player.Hunt(name);
			bot.Sleep(1000);
		}
	}

	public bool HasMaxStackItem(string[] items)
	{
		bool maxStackItem = false;

		for (int i = 0; i < items.Length; i++)
		{
			string item = items[i];

			maxStackItem = bot.Inventory.TempItems.Exists(x => x.Name == item && x.Quantity == x.MaxStack);

			if (maxStackItem)
				break;
		}

		return maxStackItem;
	}

	public void CheckQuests()
	{
		QuestIDs.ForEach(id => {
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
			}

			if (!bot.Quests.ActiveQuests.Exists(quest => quest.ID == id))
			{
				bot.Quests.EnsureAccept(id);
				bot.Wait.ForQuestAccept(id);
			}
		});
	}
}
