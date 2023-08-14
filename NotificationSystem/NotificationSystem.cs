namespace NotificationSystem
{
    using System;
    using System.Collections.Generic;

    public enum NotificationType
    {
        Status,
        News,
        Marketing
    }

    public class RateLimitRule
    {
        public NotificationType Type { get; set; }
        public int MaxCount { get; set; }
        public TimeSpan TimeWindow { get; set; }
    }

    public class RateLimiter
    {
        private Dictionary<string, Dictionary<NotificationType, Queue<DateTime>>> userLimits;

        public RateLimiter()
        {
            userLimits = new Dictionary<string, Dictionary<NotificationType, Queue<DateTime>>>();
        }

        public bool IsAllowed(string userId, NotificationType type, RateLimitRule rule)
        {
            if (!userLimits.TryGetValue(userId, out var userRules))
            {
                userRules = new Dictionary<NotificationType, Queue<DateTime>>();
                userLimits[userId] = userRules;
            }

            if (!userRules.TryGetValue(type, out var queue))
            {
                queue = new Queue<DateTime>();
                userRules[type] = queue;
            }

            var currentTime = DateTime.Now;
            while (queue.Count > 0 && currentTime - queue.Peek() > rule.TimeWindow)
            {
                queue.Dequeue();
            }

            if (queue.Count < rule.MaxCount)
            {
                queue.Enqueue(currentTime);
                return true;
            }

            return false;
        }
    }

    public class NotificationService
    {
        private readonly Gateway gateway;
        private readonly RateLimiter rateLimiter;
        private readonly Dictionary<NotificationType, RateLimitRule> rateLimitRules;

        public NotificationService(Gateway gateway)
        {
            rateLimiter = new RateLimiter();
            this.gateway = gateway;
            rateLimitRules = new Dictionary<NotificationType, RateLimitRule>
            {
                { NotificationType.Status, new RateLimitRule { MaxCount = 2, TimeWindow = TimeSpan.FromMinutes(1) } },
                { NotificationType.News, new RateLimitRule { MaxCount = 1, TimeWindow = TimeSpan.FromDays(1) } },
                { NotificationType.Marketing, new RateLimitRule { MaxCount = 3, TimeWindow = TimeSpan.FromHours(1) } }
            };
        }

        public bool SendNotification(string userId, NotificationType type, string message)
        {
            if (!rateLimitRules.TryGetValue(type, out var rule) || !rateLimiter.IsAllowed(userId, type, rule))
            {
                return false;
            }

            this.gateway.Send(userId, message);
            return true;
        }
    }

    public class Gateway
    {
        public void Send(string userId, string message)
        {
            Console.WriteLine($"Sending message to user {userId}: {message}");
        }
    }
}
