using Microsoft.CodeAnalysis;

namespace LibEternal.SourceGenerators.Helper
{
	/// <summary>
	/// A helper class that helps with creating documentation
	/// </summary>
	public static class DocumentationHelper
	{
		/// <summary>
		/// Returns an XML inheritdoc for the symbol. Does not include the preceding triple slash ('///')
		/// </summary>
		/// <param name="symbol">The symbol to reference</param>
		public static string Inheritdoc(ISymbol symbol) => $@"<inheritdoc cref=""{symbol.GetDocumentationCommentId()}""/>";
	}
}