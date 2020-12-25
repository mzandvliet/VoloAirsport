using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using RamjetAnvil.Volo.CourseEditing;
using UnityEngine;

namespace RamjetAnvil.Volo {

    public class UpgradeCoursesV1ToV2 : IUpgradeScript {

        public void Upgrade() {
            UpgradeCourses(CoursesFileStorage.CoursesDir.Value);
        }

        public static void UpgradeStockCourses() {
            UpgradeCourses(CoursesFileStorage.StockCoursesDir(Application.streamingAssetsPath));
        }

        private static void UpgradeCourses(string coursesDir) {
            var existingCourses = CourseSerialization.DeserializeCourses(coursesDir);
            foreach (var existingCourse in existingCourses) {
                if (existingCourse.FormatVersion < 2) {
                    Debug.Log("Upgrading course: '" + existingCourse.Name + "' with format version " + existingCourse.FormatVersion + " to format version 2");
                    var upgradedCourse = existingCourse;
                    upgradedCourse.Props = upgradedCourse.Props
                        .Select(prop => {
                            const float scaleFactor = 1f / 1000f * 1024f;
                            prop.Transform = prop.Transform.UpdatePosition(
                                new Vector3(
                                    x: prop.Transform.Position.x * scaleFactor,
                                    y: prop.Transform.Position.y,
                                    z: prop.Transform.Position.z * scaleFactor));
                            return prop;
                        }).ToImmutableList();
                    upgradedCourse.FormatVersion = 2;
                    CoursesFileStorage.CreateCourse(coursesDir, upgradedCourse);
                }
            }
        }
    }
}
