using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System.Threading;
using System.Globalization;

public class ScoreIncantationResponse
{
	public float[] spellScores;
}

public class MagicClient : MonoBehaviour
{
	private string incantation;
	private ScoreIncantationResponse scoreIncantationResponse;
	private System.Action<ScoreIncantationResponse> callback;
	private int request_state;

	private Thread thread;
	private bool is_running;

	public void ScoreIncantation(string incantation, System.Action<ScoreIncantationResponse> callback)
	{
		this.incantation = incantation;
		this.callback = callback;
		scoreIncantationResponse = null;
		request_state = 1;
	}

	private void Start()
	{
		thread = new Thread(Run);
		thread.Start();
		is_running = true;
	}

	private void Update()
	{
		if (request_state == 3)
		{
			callback(scoreIncantationResponse);
			request_state = 0;
		}
	}

	private void OnDestroy()
	{
		is_running = false;
		thread.Join();
	}

	private void Run()
	{
		// Prevent unity freeze after one use?
		ForceDotNet.Force(); 

		using (RequestSocket client = new RequestSocket())
		{
			client.Connect("tcp://localhost:5555");

			while (is_running)
			{
				if (request_state == 1)
				{
					client.SendFrame(incantation);
					request_state = 2;
				}
				else if (request_state == 2)
				{
					while (is_running)
					{
						if (client.TryReceiveFrameString(out string response))
						{
							if (TryParseFloatArray(response, out float[] scores))
							{
								scoreIncantationResponse = new ScoreIncantationResponse();
								scoreIncantationResponse.spellScores = scores;
							}
							else
							{
								Debug.Log(string.Format("rwdbg Unexpected response: ", response));
							}
							request_state = 3;
							break;
						}
					}
				}
			}
		}

		// Prevent unity freeze after one use?
		NetMQConfig.Cleanup();
	}

	static bool TryParseFloatArray(string input, out float[] result)
	{
		result = null;

		try
		{
			// Remove square brackets and split the input string into an array of strings
			string[] stringValues = input
				.Trim('[', ']')
				.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

			// Convert each string to a float and store in the result array
			result = new float[stringValues.Length];
			for (int i = 0; i < stringValues.Length; i++)
			{
				if (!float.TryParse(stringValues[i], out result[i]))
				{
					// Parsing failed for at least one element
					return false;
				}
			}

			// Parsing successful
			return true;
		}
		catch (System.Exception)
		{
			// An exception occurred during parsing
			return false;
		}
	}
}