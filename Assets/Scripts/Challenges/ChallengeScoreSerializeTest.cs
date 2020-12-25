using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RamjetAnvil.Volo {
    public class ChallengeScoreSerializeTest : MonoBehaviour {

        void Awake() {
            ChallengeScoring.WriteScores(new ChallengeScoring.Score(42));
            var score = ChallengeScoring.ReadScores();
            Debug.Log("score: " + score);


//            var score = new ChallengeScoring.Score();
//            score.Value = 42;
//            ChallengeScoring.Save(new ChallengeScoring.Score(), ChallengeScoring.ScoreStoragePath.Value);
//            var scoreDeserialized = new ChallengeScoring.Score();
//            ChallengeScoring.Load(ref scoreDeserialized, ChallengeScoring.ScoreStoragePath.Value);
//            Debug.Log("deserialized score " + scoreDeserialized.Value);
        }
    }
}
