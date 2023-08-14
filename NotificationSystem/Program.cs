using NotificationSystem;

var service = new NotificationService(new Gateway());
service.SendNotification("user", NotificationType.News, "Sample 1");
service.SendNotification("user", NotificationType.News, "Sample 2");
service.SendNotification("user", NotificationType.Status, "Sample 3");
service.SendNotification("user 2", NotificationType.Marketing, "Sample 4");

