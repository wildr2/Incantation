using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System.Threading;
using System.Globalization;

public class MagicClient : MonoBehaviour
{
	private string incantation;
	private float score;
	private System.Action<float> callback;
	private int request_state;

	private Thread thread;
	private bool is_running;

	public void CheckSpell(string incantation, System.Action<float> callback)
	{
		this.incantation = incantation;
		this.callback = callback;
		score = -1;
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
			callback(score);
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
							if (float.TryParse(response, NumberStyles.Any, CultureInfo.InvariantCulture, out score))
							{
								Debug.Log(string.Format("rwdbg score {0} {1}", response, score));
							}
							else
							{
								Debug.Log(string.Format("rwdbg bad score {0} {1}", response, score));
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
}