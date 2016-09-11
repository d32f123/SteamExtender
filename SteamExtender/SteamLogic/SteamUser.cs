using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamExtender.SteamLogic
{

    /// <summary>
    /// Данный класс представляет облачение пользователя Steam.
    /// Содержит в себе информацию об уникальном ID пользователя, его имени, а также его текущем статусе.
    /// TODO: В SteamMain будут содержаться методы для регистрации на изменение статуса пользователя(и др.)
    ///         Необходимо добавить регистрацию и обработчик для срабатывания данного события
    /// </summary>
    class SteamUser
    {
        private int steamID;
        private string userName;
        private SteamStatus currStatus;

        public int SteamID { get { return steamID; } }
        public string UserName { get { return userName; } }
        public SteamStatus Status { get { return currStatus; } }

        private Action<SteamStatus> propertyChanged;

        public SteamUser(int steamID) : this(steamID, null, SteamStatus.NaN) {}

        public SteamUser(int steamID, string userName, SteamStatus userStatus)
        {
            if (steamID <= 0)
                throw new ArgumentOutOfRangeException();
            if (userName == null)
                //Not implemented
                ;
            this.steamID = steamID;
            this.userName = userName;
            this.currStatus = userStatus;

            //TODO:   REGISTER FOR STATUS UPDATES + REQUEST CURR STATUS
        }

        public void SubscribeForStatusUpdate(Action<SteamStatus> method)
        {
            propertyChanged += method;
        }

        public void UnsubscribeFromStatusUpdate(Action<SteamStatus> method)
        {
            propertyChanged -= method;
        }

        private void statusChanged(object newState)
        {
            SteamStatus newStatus = (SteamStatus)newState;
            if (newStatus != currStatus)
                propertyChanged.Invoke(newStatus);
            currStatus = newStatus;
        }

    }
}
