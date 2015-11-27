using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class EventManager : MonoBehaviour
{
	public GameObject EventTimer;
	public GameObject EventPanel;
	public string EventsFilename;
	List<Event> Events;

	public void MakeActionEvent ()
	{
		//TODO Событие при переходе игрока на тайл.
	}

	public void MakeTimeEvent ()
	{
		//TODO Событие от таймера.
	}

	void LoadEventsFromFile ()
	{
		//TODO Загрузка события из бинарного файла.
		if (File.Exists (EventsFilename)) 
		{
			using (BinaryReader reader=new BinaryReader(File.Open(EventsFilename,FileMode.Open))) 
			{
				while (reader.PeekChar()!=-1)
				{
					Events.Add (new Event (
						reader.ReadString (),
						reader.ReadSingle (),
						reader.ReadSingle (),
						reader.ReadBoolean (),
						reader.ReadSingle (),
						reader.ReadSingle (),
						reader.ReadString (),
						reader.ReadSingle (),
						reader.ReadSingle ()
					));
				}
			}
		} 
		else
			throw new IOException ("File with events do not exist.");
	}
}
