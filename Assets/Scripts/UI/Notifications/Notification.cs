using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// Первый вариант
namespace UnityEngine {
	public class Notification : MonoBehaviour {
		
		[SerializeField]
		private Color ErrorColor;
		[SerializeField]
		private Color WarningColor;
		[SerializeField]
		private Color AccessColor;
		[SerializeField]
		private Color SystemColor;
        [SerializeField]
        private Text Title;
        [SerializeField]
        private Text Description;
        [SerializeField]
        private Image BackgroundImage;
        //[SerializeField]
        //private AudioClip Clip;
        public enum TYPE {
			Error,
			Warning,
			Access,
			System
        }
        public void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
		public TYPE NotificationType = TYPE.Error;
        [ContextMenu("test")]
        public void Test()
        {
			AddMessage("Предупреждение", "Вы ранены", TYPE.Error, 5f);
        }
        public void AddMessage(string title, string description, TYPE quest,float time)
        {
            CancelInvoke("DisablePanel");
            //AudioSource.PlayClipAtPoint(Clip, Camera.main.transform.position);
			this.gameObject.SetActive(true);

			if (quest == TYPE.Error)
            {
				BackgroundImage.color = ErrorColor;
            }
			if (quest == TYPE.Warning)
            {
				BackgroundImage.color = WarningColor;
            }
			if (quest == TYPE.Access)
            {
				BackgroundImage.color = AccessColor;
            }
			if (quest == TYPE.System)
			{
				BackgroundImage.color = SystemColor;
			}

            Title.text = title;
            Description.text = description;
            Invoke("DisablePanel",time);
        }

        private void DisablePanel()
        {
			this.gameObject.SetActive(false);
        }

        public static Notification getNotification
        {
            get
            {
                Notification go = GameObject.FindObjectOfType(typeof(Notification)) as Notification;
                return go;
            }
        }
	}
}
