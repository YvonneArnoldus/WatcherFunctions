using System.Collections.Generic;
using Slack;
using Social;

namespace YouTubeWatcher
{
	public static class ResultExtensions
	{
		public static Message ToSlack(this Result socialResult, string channel)
		{
			return new Message
			{
				Channel = channel,
				Attachments = new List<Attachment>
				{
					new Attachment
					{
						Title = socialResult.Title,
						AuthorName = socialResult.Author,
						AuthorLink = socialResult.AuthorUrl,
						TitleLink = socialResult.Url,
						ThumbUrl = socialResult.ThumbnailUrl,
						Text = socialResult.Description,
						Footer =  socialResult.SocialKind.Name,
						FooterIcon = socialResult.SocialKind.Icon,
						FooterTimestamp = socialResult.PublishedOn
					}
				}
			};
		}
	}
}
