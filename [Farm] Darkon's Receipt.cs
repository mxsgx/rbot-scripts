using RBot;

public class Script
{
	public ScriptInterface bot;

	public int QuestID = 7324;

	public void ScriptMain(ScriptInterface instance)
	{
		bot = instance;

		CheckItems();

		InitOptions();
		InitSkills();
		InitDrops();

		while (!bot.ShouldExit())
		{
			if (!bot.Inventory.IsMaxStack("Darkon's Receipt"))
			{
				if (bot.Map.Name != "arcangrove")
				{
					bot.Player.Join("arcangrove");
					bot.Wait.ForMapLoad("arcangrove");
				}

				CheckQuest();

				bot.Player.HuntForItem("Gorillaphant", "Banana", 22, false, bot.Drops.RejectElse);

				bot.Sleep(1500);
			}
		}

		CleanUpScript();
	}

	public void CheckItems()
	{
		bot.Player.LoadBank();
		bot.Wait.ForBankLoad();

		if (bot.Bank.Contains("Darkon's Receipt"))
		{
			bot.Bank.ToInventory("Darkon's Receipt");
		}
	}

	public void CheckQuest()
	{
		bot.Quests.EnsureLoad(QuestID);

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

		if (!bot.Quests.IsInProgress(QuestID))
		{
			bot.Quests.EnsureAccept(QuestID);
			bot.Wait.ForQuestAccept(QuestID);
		}
	}

	public void InitDrops()
	{
		bot.Drops.Pickup.Clear();

		bot.Drops.Add("Darkon's Receipt");

		bot.Drops.Start();
	}

	public void InitOptions()
	{
		bot.Options.SafeTimings = true;
		bot.Options.RestPackets = true;
		bot.Options.InfiniteRange = true;
		bot.Options.ExitCombatBeforeQuest = true;
		bot.Options.SkipCutscenes = true;
	}

	public void InitSkills()
	{
		bot.Skills.SkillTimeout = 5;
		bot.Skills.SkillTimer = 2000;

		bot.Skills.Clear();

		bot.Skills.Add(1);
		bot.Skills.Add(2);
		bot.Skills.Add(3);
		bot.Skills.Add(4);

		bot.Skills.StartTimer();
	}

	public void CleanUpScript()
	{
		bot.Skills.Stop();
		bot.Skills.Clear();

		bot.Drops.Stop();
		bot.Drops.Pickup.Clear();
	}
}