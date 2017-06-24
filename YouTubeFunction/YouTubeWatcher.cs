using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace YouTubeWatcher
{
	public static class YouTubeWatcher
	{
		[FunctionName("YouTubeWatcher")]
		public static void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, TraceWriter log)	
		{
			try
			{
				// read options
				var slackOptions = new Slack.Options
				{
					WebhookUrl = ConfigurationManager.AppSettings["Slack:WebhookUrl"]
				};
				var youTubeOptions = new Youtube.Options
				{
					ApiKey = ConfigurationManager.AppSettings["YouTube:ApiKey"],
					ApplicationName = ConfigurationManager.AppSettings["YouTube:ApplicationName"]
				};
				var searchOptions = new Youtube.SearchOptions
				{
					Options = JsonConvert.DeserializeObject<List<Youtube.Option>>(ConfigurationManager.AppSettings["SearchOptions"])
				};

				// get last run date
				var lastRunDate = myTimer.ScheduleStatus.Last.ToUniversalTime();
				log.Info($"lastRunDate: {lastRunDate:F}");

				// create clients
				var youtubeClient = new Youtube.Client(new Microsoft.Extensions.Options.OptionsWrapper<Youtube.Options>(youTubeOptions));
				var slackClient = new Slack.Client(new Microsoft.Extensions.Options.OptionsWrapper<Slack.Options>(slackOptions));

				// run
				var watcher = new Watcher(slackClient, youtubeClient, new Microsoft.Extensions.Options.OptionsWrapper<Youtube.SearchOptions>(searchOptions), log);
				watcher.Process(lastRunDate);

				var response = slackClient.PostAsync(new Slack.Message { Text = $"Social Search Executed with published after {lastRunDate}" }).Result;
			}
			catch (Exception e)
			{
				log.Error($"Exception {e.Message}");
			}
		}
	}
}