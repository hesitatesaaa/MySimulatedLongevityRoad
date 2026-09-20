using System;
using RSG;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopUploadingWorldWindow : MonoBehaviour
{
	public Button doneButton;

	public Image loadingImage;

	public Image doneImage;

	public Image errorImage;

	public GameObject barParent;

	public Text statusMessage;

	public Text percents;

	public Image bar;

	public Image mask;

	public static bool uploading;

	public static bool needsWorkshopAgreement;

	public GameObject workshopAgreementButton;

	private unsafe void OnEnable()
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.game_loaded)
		{
			return;
		}
		needsWorkshopAgreement = false;
		((Component)errorImage).gameObject.SetActive(false);
		((Component)doneButton).gameObject.SetActive(false);
		workshopAgreementButton.gameObject.SetActive(false);
		statusMessage.text = LocalizedTextManager.getText("uploading_your_world");
		((Component)loadingImage).gameObject.SetActive(true);
		((Component)doneImage).gameObject.SetActive(false);
		((Component)bar).gameObject.SetActive(true);
		((Component)percents).gameObject.SetActive(true);
		((Component)mask).gameObject.SetActive(true);
		barParent.SetActive(true);
		((Component)bar).transform.localScale = new Vector3(0f, 1f, 1f);
		uploading = true;
		SteamSDK.steamInitialized.Then((Func<IPromise>)(() => (IPromise)(object)WorkshopMaps.uploadMap())).Then((Action)delegate
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			progressBarUpdate();
			uploading = false;
			((Component)doneButton).gameObject.SetActive(true);
			statusMessage.text = LocalizedTextManager.getText("world_uploaded");
			((Component)loadingImage).gameObject.SetActive(false);
			((Component)doneImage).gameObject.SetActive(true);
			if (needsWorkshopAgreement)
			{
				statusMessage.text = LocalizedTextManager.getText("workshop_agreement");
				workshopAgreementButton.SetActive(true);
			}
			else
			{
				PublishedFileId uploaded_file_id = WorkshopMaps.uploaded_file_id;
				Application.OpenURL("steam://url/CommunityFilePage/" + ((object)(*(PublishedFileId*)(&uploaded_file_id))/*cast due to constrained. prefix*/).ToString());
			}
			barParent.SetActive(false);
			((Component)bar).gameObject.SetActive(false);
			((Component)percents).gameObject.SetActive(false);
			((Component)mask).gameObject.SetActive(false);
		}).Catch((Action<Exception>)delegate(Exception e)
		{
			statusMessage.text = LocalizedTextManager.getText("upload_error") + "\n( " + e.Message.ToString() + " )";
			uploading = false;
			Debug.LogError((object)e.Message.ToString());
			((Component)doneButton).gameObject.SetActive(true);
			((Component)doneImage).gameObject.SetActive(false);
			((Component)loadingImage).gameObject.SetActive(false);
			((Component)errorImage).gameObject.SetActive(true);
		});
	}

	private void Update()
	{
		if (uploading || ((Behaviour)percents).isActiveAndEnabled)
		{
			progressBarUpdate();
		}
	}

	private void progressBarUpdate()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		float uploadProgress = WorkshopMaps.uploadProgress;
		float x = ((Component)bar).transform.localScale.x;
		if (((Component)bar).transform.localScale.x < uploadProgress)
		{
			x = ((Component)bar).transform.localScale.x + Time.deltaTime;
			if (x > uploadProgress || uploadProgress > 0.75f)
			{
				x = uploadProgress;
			}
			((Component)bar).transform.localScale = new Vector3(x, 1f, 1f);
			percents.text = Mathf.CeilToInt(x * 100f) + " %";
		}
		else
		{
			percents.text = Mathf.CeilToInt(uploadProgress * 100f) + " %";
		}
	}
}
