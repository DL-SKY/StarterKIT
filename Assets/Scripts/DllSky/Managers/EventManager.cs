/*
© Alexander Danilovsky, 2018
----------------------------
= Event Manager =
*/

using System;

namespace DllSky.Managers
{
    public static class EventManager
    {
        #region Actions
        public static Action eventOnDefault;
        public static Action eventOnClickEsc;

        public static Action eventOnChangeLanguage;
        public static Action eventOnApplyLanguage;

        public static Action<string> eventOnResourceUpdate;
        #endregion

        #region Public methods
        public static void CallOnDefault()
        {
            eventOnDefault?.Invoke();
        }

        public static void CallOnClickEsc()
        {
            eventOnClickEsc?.Invoke();
        }

        public static void CallOnChangeLanguage()
        {
            eventOnChangeLanguage?.Invoke();
        }

        public static void CallOnApplyLanguage()
        {
            eventOnApplyLanguage?.Invoke();
        }

        public static void CallOnResourceUpdate(string _resID)
        {
            eventOnResourceUpdate?.Invoke(_resID);
        }
        #endregion
    }
}