using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class EventManager : MonoBehaviour
{
	public GameObject EventTimer;
	public GameObject EventPanel;
	public GameObject ReactionButtonPanel;
	public string EventsFilename;
	public GameObject Btn;
	List<Event> Events = new List<Event> ();
	List<GameObject> ReactionButtons = new List<GameObject> ();
	Event CurrentEvent;

	void Start()
	{
		LoadEventsFromFile();
	}

	public void MakeActionEvent ()
	{
		//TODO Событие при переходе игрока на тайл.
		List<Event> possibleEvents = Events.Where (evnt => evnt.ByAction).ToList();// UNDONE Не учитываются коэффициенты.

		//while (true) // Если раскомментировать, то событие будет выбрано в любом случае (возможны баги).
		//{
			List<Event> eventLst = possibleEvents;
			while (eventLst.Count!=0) 
			{
				Event evnt = eventLst [Random.Range (0, eventLst.Count)];
				if (Random.value < evnt.Probability) // UNDONE Не учитываются коэффициенты.
				{
					CurrentEvent=evnt;
					EventPanel.SetActive (true);
					EventPanel.transform.GetChild(0).GetComponent<Text>().text = evnt.Description;

					sbyte d =(sbyte) (evnt.Reactions.Count - ReactionButtons.Count - 1);
					if (d > 0) 
						for (byte i=0; i<d; ++i) 
						{
							GameObject newBtn = Instantiate (Btn);
							newBtn.GetComponent<EventButtonTest> ().Index =(byte)( ReactionButtons.Count!=0? ReactionButtons[ReactionButtons.Count-1].GetComponent<EventButtonTest> ().Index+1:1);
							newBtn.transform.SetParent (ReactionButtonPanel.transform);
							ReactionButtons.Add (newBtn);
						}
					else if (d < 0) 
						for (sbyte i=-1; i>=d; --i)
						{
							Destroy (ReactionButtons [ReactionButtons.Count + i]);
							ReactionButtons.RemoveAt (ReactionButtons.Count + i);
						}
					//ReactionButtons.RemoveRange(ReactionButtons.Count-Mathf.Abs(d),Mathf.Abs(d));

					Btn.transform.GetChild(0).GetComponent<Text> ().text=evnt.Reactions [0].Description;
					for (byte i=0; i<ReactionButtons.Count; ++i) 
						ReactionButtons [i].transform.GetChild(0).GetComponent<Text> ().text = evnt.Reactions [i+1].Description;
					return;
				} 
				else 
					eventLst.Remove (evnt);
			}
		//}
	}

	public void MakeTimeEvent ()
	{
		//TODO Событие от таймера.
	}

	public void EventPanelButtonPress (byte index)
	{
		Debug.Log(CurrentEvent.Reactions[index].ResultDescription);
		EventPanel.SetActive (false);
		CurrentEvent=null;
	}

	void LoadEventsFromFile ()
	{
		if (File.Exists (EventsFilename)) 
		{
			using (BinaryReader reader = new BinaryReader(File.Open(EventsFilename, FileMode.Open))) 
			{
				while (reader.PeekChar() != -1) 
				{
					string name = reader.ReadString ();
					bool good = reader.ReadBoolean ();
					bool byAction = reader.ReadBoolean ();
					bool byTime = reader.ReadBoolean ();
					string description = reader.ReadString ();
					float probability = reader.ReadSingle ();
					
					Event.TerrainCoefficients terrCoef = new Event.TerrainCoefficients ()
					{
						ForestСoef = reader.ReadSingle(),
						RiverСoef = reader.ReadSingle()
					};
					Event.TimeCoefficients timeCoef = new Event.TimeCoefficients ()
					{
						DayСoef = reader.ReadSingle(),
						NightСoef = reader.ReadSingle()
					};
					Event.StateCoefficients stateCoef = new Event.StateCoefficients ();
					//
					
					byte reactionCount = reader.ReadByte ();
					List<Event.Reaction> reactions = new List<Event.Reaction> (reactionCount);
					for (byte i = 0; i < reactionCount; ++i) {
						reactions.Add (new Event.Reaction ()
						              {
							Description = reader.ReadString(),
							ResultDescription = reader.ReadString(),
							VitalityEffect = reader.ReadSingle()
						});
					}
					
					Events.Add (new Event (name, good, byAction, byTime, description, probability, terrCoef, timeCoef, stateCoef, reactions));
				}
			}
		} else
			throw new IOException ("File with events do not exist.");
	}
}
