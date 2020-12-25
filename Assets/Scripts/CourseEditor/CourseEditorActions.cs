using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using RamjetAnvil.Unity.Utility;

namespace RamjetAnvil.Volo.CourseEditing
{
    public class CourseEditorActions {
        private readonly ISubject<string> _updateName;
        private readonly ISubject<Unit> _createProp;
        private readonly ISubject<ImmutableTransform> _createPropOnLocation; 
        private readonly ISubject<PropId> _deleteProp;
        private readonly ISubject<Unit> _deleteSelectedProp;
        private readonly ISubject<Tuple<PropId, ImmutableTransform>> _updateProp;
        private readonly ISubject<IImmutableList<PropId>> _reorderProps; 
        private readonly ISubject<Maybe<PropId>> _selectProp;
        private readonly ISubject<Maybe<PropId>> _highlightProp;
        private readonly ISubject<PropType> _selectPropType;
        private readonly ISubject<Unit> _undo;
        private readonly ISubject<Unit> _redo;
        private readonly ISubject<TransformTool> _selectTransformTool;
        private readonly ISubject<PropId> _moveToProp;

        public CourseEditorActions() {
            _updateName = new Subject<string>();
            _createProp = new Subject<Unit>();
            _createPropOnLocation = new Subject<ImmutableTransform>();
            _deleteProp = new Subject<PropId>();
            _deleteSelectedProp = new Subject<Unit>();
            _updateProp = new Subject<Tuple<PropId, ImmutableTransform>>();
            _reorderProps = new Subject<IImmutableList<PropId>>();
            _selectProp = new Subject<Maybe<PropId>>();
            _highlightProp = new Subject<Maybe<PropId>>();
            _selectPropType = new Subject<PropType>();
            _undo = new Subject<Unit>();
            _redo = new Subject<Unit>();
            _selectTransformTool = new Subject<TransformTool>();
            _moveToProp = new Subject<PropId>();
        }

        public ISubject<string> UpdateName {
            get { return _updateName; }
        }

        public ISubject<Unit> CreateProp
        {
            get { return _createProp; }
        }

        public ISubject<ImmutableTransform> CreatePropOnLocation {
            get { return _createPropOnLocation; }
        }

        public ISubject<PropId> DeleteProp {
            get { return _deleteProp; }
        }

        public ISubject<Unit> DeleteSelectedProp {
            get { return _deleteSelectedProp; }
        }

        public ISubject<Maybe<PropId>> SelectProp
        {
            get { return _selectProp; }
        }

        public ISubject<Maybe<PropId>> HighlightProp
        {
            get { return _highlightProp; }
        }

        public ISubject<Unit> Undo {
            get { return _undo; }
        }

        public ISubject<Unit> Redo {
            get { return _redo; }
        }

        public ISubject<PropType> SelectPropType {
            get { return _selectPropType; }
        }

        public ISubject<Tuple<PropId, ImmutableTransform>> UpdateProp {
            get { return _updateProp; }
        }

        public ISubject<IImmutableList<PropId>> ReorderProps {
            get { return _reorderProps; }
        }

        public ISubject<PropId> MoveToProp {
            get { return _moveToProp; }
        }

        public ISubject<TransformTool> SelectTransformTool {
            get { return _selectTransformTool; }
        }
    }
}
