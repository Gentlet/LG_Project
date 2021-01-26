using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HVR {


public interface ITimestampProvider {
	bool ProvidesTimestamp();
	float GetTimestamp();
	void ConnectHvrActor(HvrActor hvrActor);
	void DisconnectHvrActor();

	void Play();
	void Pause();
	void Stop();
	void Seek(float time);
	void SetLooping(bool loop);
	bool IsLooping();
	float GetDuration();

	void MarkAsCaching(bool caching);
	bool IsCaching();
}

}
