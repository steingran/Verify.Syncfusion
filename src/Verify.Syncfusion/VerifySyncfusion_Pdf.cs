﻿namespace VerifyTests;

public static partial class VerifySyncfusion
{
    static ConversionResult ConvertPdf(Stream stream, IReadOnlyDictionary<string, object> settings)
    {
        using var document = new PdfLoadedDocument(stream);

        return ConvertPdf(document, settings);
    }

    static ConversionResult ConvertPdf(PdfDocument document, IReadOnlyDictionary<string, object> settings)
    {
        var info = GetInfo(document, document.DocumentInformation);
        return new(info, GetPdfStreams(document, settings).ToList());
    }

    static ConversionResult ConvertPdf(PdfLoadedDocument document, IReadOnlyDictionary<string, object> settings)
    {
        var info = GetInfo(document, document.DocumentInformation);
        return new(info, GetPdfStreams(document, settings).ToList());
    }

    static object GetInfo(PdfDocumentBase document, PdfDocumentInformation documentDocumentInformation)
    {
        var info = documentDocumentInformation;
        if (info.Title == "Syncfusion" ||
            info.Subject == "Syncfusion" ||
            info.Author == "Syncfusion")
        {
            throw new("The default value of 'Syncfusion' for Title, Subject, or Author is not allowed.");
        }

        return new
        {
            document.PageCount,
            info.Author,
            info.CreationDate,
            info.Creator,
            info.CustomMetadata,
            info.Keywords,
            info.Language,
            info.ModificationDate,
            info.Producer,
            info.Subject,
            info.Title,
        };
    }

    static IEnumerable<Target> GetPdfStreams(PdfDocument document, IReadOnlyDictionary<string, object> settings)
    {
        var pages = document.Pages.Cast<PdfPageBase>().ToList();
        return GetPdfStreams(document, settings, pages);
    }

    static IEnumerable<Target> GetPdfStreams(PdfLoadedDocument document, IReadOnlyDictionary<string, object> settings)
    {
        var pages = document.Pages.Cast<PdfPageBase>().ToList();
        return GetPdfStreams(document, settings, pages);
    }

    static IEnumerable<Target> GetPdfStreams(
        PdfDocumentBase document,
        IReadOnlyDictionary<string, object> settings,
        List<PdfPageBase> pages)
    {
        var pagesToInclude = settings.GetPagesToInclude(pages.Count);
        var pdfStream = new MemoryStream();
        document.Save(pdfStream);
        pdfStream.Position = 0;
        var pngDevice = settings.GetPdfPngDevice(document);
        pngDevice.Load(pdfStream);
        for (var index = 0; index < pagesToInclude; index++)
        {
            var page = pages[index];
            var text = page.ExtractText();
            yield return new("txt", text);
            //TODO: also export page text
            var pngStream = new MemoryStream();
#if NET6_0_OR_GREATER
            var image = pngDevice.ExportAsImage(index);
            var skData = image.Encode(SKEncodedImageFormat.Png,100);
            skData.SaveTo(pngStream);
            #else
            var bitmap = pngDevice.ExportAsImage(index);
            bitmap.Save(pngStream, ImageFormat.Png);
#endif
            yield return new("png", pngStream);
        }
    }
}