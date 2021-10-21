using RBot;

public class Script
{
	public ScriptInterface bot;

	public int QuestID = 2934;

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

		while (!bot.ShouldExit())
		{
			if (bot.Player.GetFactionRank("Blade of Awe") < 10)
			{
				if (bot.Map.Name != "gilead")
					bot.Player.Join("gilead");

				CheckQuest();

				bot.Player.HuntForItem("Fire Elemental|Water Elemental|Earth Elemental|Wind Elemental", "Handle Found!", 1, false, bot.Drops.RejectElse);

				bot.Sleep(1500);
			}
		}

		bot.Skills.Clear();
		bot.Skills.StopTimer();
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