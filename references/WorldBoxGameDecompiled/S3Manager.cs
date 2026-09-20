using System;
using System.IO;
using System.Net;
using System.Threading;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using RSG;
using SAES;
using UnityEngine;

public class S3Manager : MonoBehaviour
{
	public static S3Manager instance;

	private const string ABN = "Js23DGKu7RMNik4XECoQVkNFIW//dsZNcfyKb49RlFU";

	private const string AAK = "VvrCEe1TcUBvQeiSelndpl1Plc4FoMxddSglHA2Fe0M";

	private const string ASK = "WVbbIlYTAH37Glxvl1MSDpKPffhczwdbi5FRgkSs8mkLEuLzE6YCiouHH71vVgLS";

	private string _abnn;

	private IAmazonS3 _s3_client;

	private IAmazonS3 _client
	{
		get
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			if (_s3_client == null)
			{
				try
				{
					SAES val = new SAES();
					_abnn = val.ToString("Js23DGKu7RMNik4XECoQVkNFIW//dsZNcfyKb49RlFU");
					_s3_client = (IAmazonS3)new AmazonS3Client(val.ToString("VvrCEe1TcUBvQeiSelndpl1Plc4FoMxddSglHA2Fe0M"), val.ToString("WVbbIlYTAH37Glxvl1MSDpKPffhczwdbi5FRgkSs8mkLEuLzE6YCiouHH71vVgLS"), RegionEndpoint.USEast2);
					val.init(false);
				}
				catch (Exception ex)
				{
					Debug.LogError((object)"s3 manager not working");
					Debug.LogError((object)ex.Message);
				}
			}
			return _s3_client;
		}
	}

	private void Start()
	{
		instance = this;
		Config.upload_available = false;
	}

	public Promise<string> uploadFileToAWS3(string pFileName, byte[] pFileRawBytes)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		Promise<string> val = new Promise<string>();
		try
		{
			MemoryStream inputStream = new MemoryStream(pFileRawBytes);
			PutObjectRequest val2 = new PutObjectRequest
			{
				BucketName = _abnn,
				Key = pFileName,
				InputStream = inputStream,
				CannedACL = S3CannedACL.Private
			};
			if (((AmazonWebServiceResponse)_client.PutObjectAsync(val2, default(CancellationToken)).WaitAndUnwrapException()).HttpStatusCode == HttpStatusCode.OK)
			{
				val.Resolve(val2.Key);
			}
			else
			{
				val.Reject(new Exception("Error when uploading!"));
			}
		}
		catch (WebException ex)
		{
			using (Stream stream = ex.Response.GetResponseStream())
			{
				using StreamReader streamReader = new StreamReader(stream);
				Debug.Log((object)streamReader.ReadToEnd());
			}
			val.Reject((Exception)ex);
		}
		catch (Exception ex2)
		{
			val.Reject(ex2);
		}
		return val;
	}
}
