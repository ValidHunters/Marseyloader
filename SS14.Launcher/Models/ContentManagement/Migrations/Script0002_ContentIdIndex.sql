-- Create an index for ContentManifest.ContentId
-- Used when clearing out unused Content blobs.

CREATE INDEX ContentManifest_ContentId ON ContentManifest(ContentId);
