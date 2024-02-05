using Microsoft.Extensions.Options;
using RopeParison.Migrations;
using RopeParison.Protocol;
using Syncfusion.Blazor.Navigations;

namespace RopeParison.SidebarHelpers
{
    public interface ISidebarService
    {
        void SidebarToggleOpen(ref bool sidebarOpen);
        void CloseSidebar(out bool sidebarOpen);
        void SidebarSetToggleText(out string sidebarToggleText, bool sidebarOpen, string hideOptionsText, string showOptionsText);
        void SetSidebarSettings(
            bool isSmall,
            bool isInit,
            ref bool sidebarOpen,
            out bool sidebarCloseOnDocumentClick,
            out bool sidebarEnableGestures,
            out bool sidebarShowBackdrop,
            out SidebarType sidebarType,
            out string showOptionsText,
            out string hideOptionsText,
            in string OPTIONS,
            in string SHOW_OPTIONS,
            in string HIDE_OPTIONS,
            out string sidebarToggleText);
    }

    public class SidebarService : ISidebarService
    {

        //-------------------------------------------------------------------------

        public void SidebarToggleOpen(ref bool sidebarOpen)
        {
            if (sidebarOpen)
            {
                sidebarOpen = false;
            }
            else
            {
                sidebarOpen = true;
            }
        }

        public void CloseSidebar(out bool sidebarOpen)
        {
            sidebarOpen = false;
        }

        public void SidebarSetToggleText(out string sidebarToggleText, bool sidebarOpen, string hideOptionsText, string showOptionsText)
        {
            if (sidebarOpen)
            {
                sidebarToggleText = hideOptionsText;
            }
            else
            {
                sidebarToggleText = showOptionsText;
            }
        }
        public void SetSidebarSettings(
            bool isSmall,
            bool isInit,
            ref bool sidebarOpen,
            out bool sidebarCloseOnDocumentClick,
            out bool sidebarEnableGestures,
            out bool sidebarShowBackdrop,
            out SidebarType sidebarType,
            out string showOptionsText,
            out string hideOptionsText,
            in string OPTIONS,
            in string SHOW_OPTIONS,
            in string HIDE_OPTIONS,
            out string sidebarToggleText)
        {
            if (isSmall)
            {
                if (isInit)
                {
                    sidebarOpen = true; //Decided to open with sidebar in small mode.
                }
                sidebarCloseOnDocumentClick = true;
                sidebarEnableGestures = true;
                sidebarShowBackdrop = true;
                sidebarType = SidebarType.Over; //For some reason changing sidebar type causes sidebar to close if it's open. No resolution as yet.
                showOptionsText = OPTIONS;
                hideOptionsText = OPTIONS;
                sidebarToggleText = showOptionsText;
            }
            else
            {
                if (isInit)
                {
                    sidebarOpen = true;
                }
                sidebarCloseOnDocumentClick = false;
                sidebarEnableGestures = false;
                sidebarShowBackdrop = false;
                sidebarType = SidebarType.Push;
                showOptionsText = SHOW_OPTIONS;
                hideOptionsText = HIDE_OPTIONS;
                SidebarSetToggleText(out sidebarToggleText, sidebarOpen, hideOptionsText, showOptionsText);
            }
        }

    }
}
