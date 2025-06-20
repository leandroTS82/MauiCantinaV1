using Android.Content;
using Android.Media;
using Android.OS;

namespace CantinaV1.Platforms.Android
{
    public static class NotificationHelper
    {
        public static void PlayNotification()
        {
            try
            {
                
                Context context = global::Android.App.Application.Context;

                // Som de notificação
                var notificationUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                var ringtone = RingtoneManager.GetRingtone(context, notificationUri);
                ringtone.Play();

                // Vibração
                var vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
                if (vibrator != null)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        vibrator.Vibrate(VibrationEffect.CreateOneShot(200, VibrationEffect.DefaultAmplitude));
                    else
                        vibrator.Vibrate(200);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao emitir notificação: " + ex.Message);
            }
        }
    }
}
