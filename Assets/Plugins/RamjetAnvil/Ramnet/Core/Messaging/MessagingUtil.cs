using System;

namespace RamjetAnvil.RamNet {
    public delegate void Try(double currentTime);

    public static class MessagingUtil {

        public static Try RateLimiter(int sendRate, Action action) {
            return RateLimiter(sendInterval: 1f / sendRate, action: action);
        }

        public static Try RateLimiter(float sendInterval, Action action) {
            double lastSendTime = sendInterval * -2;
            return currentTime => {
                if (currentTime > lastSendTime + sendInterval) {
                    action();
                    lastSendTime = currentTime;
                }
            };
        }
    }
}
