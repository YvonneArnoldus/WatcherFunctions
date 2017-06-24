using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Options;

namespace YouTubeWatcher
{
	public class Watcher
	{
		private readonly TraceWriter _log;
		private readonly Slack.Client _slackClient;
		private readonly Youtube.Client _youtubeClient;
		private readonly IOptions<Youtube.SearchOptions> _searchOptions;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="youtubeClient"></param>
		/// <param name="searchOptions"></param>
		/// <param name="slackClient"></param>
		/// <param name="log"></param>
		public Watcher(Slack.Client slackClient, Youtube.Client youtubeClient, IOptions<Youtube.SearchOptions> searchOptions, TraceWriter log)
		{
			_slackClient = slackClient ?? throw new ArgumentNullException(nameof(slackClient));
			_youtubeClient = youtubeClient ?? throw new ArgumentNullException(nameof(youtubeClient));
			_searchOptions = searchOptions ?? throw new ArgumentNullException(nameof(searchOptions));
			_log = log ?? throw new ArgumentNullException(nameof(log));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lastRunDate"></param>
		/// <returns></returns>
		public async Task Process(DateTime? lastRunDate)
		{
			var options = _searchOptions.Value.Options;
			foreach (var option in options)
			{
				_log.Info($"channel: {option.SlackChannel}");
				if (lastRunDate == null)
					lastRunDate = option.PublishedAfter.ToUniversalTime();
				if (lastRunDate == DateTime.MinValue)
					lastRunDate = option.PublishedAfter.ToUniversalTime();

				_log.Info($"lastRunDate: {lastRunDate:F}");
				foreach (var keyword in option.Keywords)
				{
					_log.Info($"keyword: {keyword}");
					var results = await _youtubeClient.SearchAsync(keyword, lastRunDate);
					foreach (var result in results)
						await _slackClient.PostAsync(result.ToSlack(option.SlackChannel));
				}
			}
		}
	}
}
