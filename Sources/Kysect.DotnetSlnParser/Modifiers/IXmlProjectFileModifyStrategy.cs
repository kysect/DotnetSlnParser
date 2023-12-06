﻿using Microsoft.Language.Xml;
using System.Collections.Generic;

namespace Kysect.DotnetSlnParser.Modifiers;

public interface IXmlProjectFileModifyStrategy<TSyntax>
    where TSyntax : SyntaxNode
{
    IReadOnlyCollection<TSyntax> SelectNodeForModify(XmlDocumentSyntax document);
    SyntaxNode ApplyChanges(TSyntax syntax);
}