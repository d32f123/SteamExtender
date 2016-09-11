using SteamExtender.SteamLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SteamExtender.AvatarHandling
{

    /// <summary>
    /// Класс ViewModel, работает с данным ему гридом
    /// устанавливает цвет border, качает картинку, регистрируется на обновления у SteamUser
    /// </summary>
    public class Avatar
    {
        private Image avatarImage;
        private BitmapImage bitmapAvatarImage;

        private Style borderStyle;

        private Border border;

        private SteamStatus currentStatus;

        private int userID;
        private string userName;

        private SteamUser user;

        public Avatar(int userID, string userName, SteamStatus currStatus)
        {
            user = new SteamUser(userID, userName, currStatus);
            this.userID = userID;
            this.userName = userName;
        }
    }
}
