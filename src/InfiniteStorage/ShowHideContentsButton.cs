namespace InfiniteStorage
{
    public class ShowHideContentsButton : KMonoBehaviour
    {
        public bool showContents;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            Subscribe((int) GameHashes.RefreshUserMenu, OnRefresh);
            GetComponent<Storage>().showInUI = showContents;
        }

        public void OnRefresh(object _)
        {
            // If we are currently showing, display hide strings
            var showContentsStr = showContents
                ? STRINGS.UI.SHOWHIDE_CONTENTS.HIDE
                : STRINGS.UI.SHOWHIDE_CONTENTS.SHOW;

            var showContentsTooltip = showContents
                ? STRINGS.UI.SHOWHIDE_CONTENTS.HIDE_TOOLTIP
                : STRINGS.UI.SHOWHIDE_CONTENTS.SHOW_TOOLTIP;

            var buttonInfo = new KIconButtonMenu.ButtonInfo(
                "action_building_disabled",
                showContentsStr,
                OnChangeShowContents,
                Action.NumActions,
                null,
                null,
                null,
                showContentsTooltip
            );

            Game.Instance.userMenu.AddButton(gameObject, buttonInfo);
        }

        private void OnChangeShowContents()
        {
            showContents = !showContents;
            GetComponent<Storage>().showInUI = showContents;
        }
    }
}
