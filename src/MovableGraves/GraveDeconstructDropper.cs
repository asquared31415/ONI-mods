using KSerialization;
using UnityEngine;

namespace MovableGraves;

[SerializationConfig(MemberSerialization.OptIn)]
public class GraveDeconstructDropper : KMonoBehaviour {
    private static readonly EventSystem.IntraObjectHandler<GraveDeconstructDropper> OnRefreshMenuHandler =
        new((c, _) => c.OnRefreshMenu());

    private static readonly EventSystem.IntraObjectHandler<GraveDeconstructDropper> OnDestroyObjectHandler =
        new((c, _) => c.OnDestroyObject());

    protected override void OnSpawn() {
        base.OnSpawn();
        Subscribe((int)GameHashes.QueueDestroyObject, OnDestroyObjectHandler);
        Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshMenuHandler);
    }

    private void OnRefreshMenu() {
        if (!TryGetComponent<Grave>(out var grave)) {
            return;
        }

        // already empty graves have a burial time of -1, don't put the button on empty graves
        if (!(grave.burialTime > -1f)) {
            return;
        }

        Game.Instance.userMenu.AddButton(
            gameObject,
            new KIconButtonMenu.ButtonInfo(
                "",
                "Empty Grave",
                () => {
                    Util.KInstantiateUI<ConfirmDialogScreen>(
                        ScreenPrefabs.Instance.ConfirmDialogScreen.gameObject,
                        GameScreenManager.Instance.ssOverlayCanvas,
                        true
                    ).PopupConfirmDialog(
                        "Are you sure you wish to delete the contents of this grave? The duplicant and their burial info will be lost <b>forever</b>!",
                        () => {
                            grave.graveName = null;
                            // epitaph normally decided on construction but clearing the grave should reset it 
                            grave.epitaphIdx = Random.Range(0, int.MaxValue);
                            grave.burialTime = -1f;

                            grave.smi.GoTo(grave.smi.sm.empty);
                        },
                        () => { }
                    );
                }
            )
        );
    }

    private void OnDestroyObject() {
        // this should be incredibly unlikely, but future-proof it
        if (this == null || !TryGetComponent<Grave>(out var grave)) {
            return;
        }

        // only drop for active graves
        if (grave.burialTime < 0) {
            return;
        }

        var infoGo = Util.KInstantiate(
            Assets.GetPrefab(GraveInfoItemConfig.ID),
            transform.position + new Vector3(0, 0.75f, 0)
        );
        var info = infoGo.GetComponent<GraveInfoItem>();
        info.SetInfo(grave.graveName, grave.epitaphIdx, grave.burialTime, grave.graveAnim);
        infoGo.SetActive(true);
    }
}
