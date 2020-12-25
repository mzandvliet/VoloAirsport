using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Volo.CourseEditing
{
    public class CourseEditorGuiState {
        public IList<GuiCourse> Courses { get; set; }
        public bool IsUndoDeleteCoursePossible { get; set; }
        public SelectedCourseGuiState SelectedCourse { get; set; }

        public CourseEditorGuiState() {
            Courses = new List<GuiCourse>();
            IsUndoDeleteCoursePossible = false;
            SelectedCourse = null;
        }
    }

    public class SelectedCourseGuiState {
        public PropType SelectedPropType { get; set; }
        public IList<GuiProp> CourseProps { get; set; }
        public string CourseName { get; set; }
        public TransformTool SelectedTool { get; set; }
        public bool IsUndoPossible { get; set; }
        public bool IsRedoPossible { get; set; }
        public string HighlightedPropId { get; set; }
        public string SelectedPropId { get; set; }

        public SelectedCourseGuiState() {
            CourseProps = new List<GuiProp>();
        }
    }

    public struct GuiCourse {
        public string Id;
        public string Name;
    }

    public struct GuiProp {
        public string Id;
        public int Height;
    }
}
