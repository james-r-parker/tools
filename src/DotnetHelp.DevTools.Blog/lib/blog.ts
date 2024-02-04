import { fetchCmsContent } from "@/lib/cms";
import { Metadata } from "next";

export type BlogPost = {
    createdAt: string,
    slug: string,
    tags: string[],
    title: string,
    overview: string,
    category: string,
    createdBy: {
        name: string
    }
    body: {
        json: any
    }
}

export type BlogOverview = {
    createdAt: string,
    slug: string,
    title: string,
    category: string
}

export type BlogListItem = {
    createdAt: string,
    slug: string,
    tags: string[],
    title: string,
    overview: string,
    category: string,
    metadata: {
        title: string,
        description: string,
        image: {
            width: number,
            height: number,
            url: string
        }
    }
}

type CmsBlogListResponse = {
    data: {
        blogs: BlogListItem[]
    }
};

type CmsBlogOverviewResponse = {
    data: {
        blogs: BlogOverview[]
    }
};

type CmsBlogMetaResponse = {
    data: {
        blog: {
            metadata: {
                title: string,
                description: string,
                image: {
                    width: number,
                    height: number,
                    url: string
                }
            },
            title: string,
            overview: string,
            category: string,
            createdBy: {
                name: string
            },
            tags: string[],
            createdAt: string,
            updatedAt: string,
        }
    }
};

type CmsGetBlogPostResponse = {
    data: {
        blog: BlogPost
    }
};

type CmsMenuItem = {
    text: string,
    link: string,
}

export type CmsMenu = { [key: string]: CmsMenuItem[] }

export async function getBlogPostOptions(): Promise<BlogOverview[]> {
    try {

        const result = await fetchCmsContent<CmsBlogOverviewResponse>('query Blogs { blogs { createdAt, slug, title, category } }');
        if (!result) {
            return [];
        }

        return result.data.blogs;
    }
    catch (err) {
        console.error('Failed to build blog menu', err);
    }

    return [];
}

export async function buildBlogMenu(): Promise<CmsMenu> {
    try {

        const result = await getBlogPostOptions();
        if (!result) {
            return {};
        }

        return result.reduce((acc, blog) => {
            if (!acc[blog.category]) {
                acc[blog.category] = [];
            }
            acc[blog.category].push({
                text: blog.title,
                link: `/blog/${blog.slug}`
            });
            return acc;
        }, {} as CmsMenu);
    }
    catch (err) {
        console.error('Failed to build blog menu', err);
    }

    return {};
}

export async function getBlogPost(slug: string): Promise<BlogPost | null> {
    try {

        const result = await fetchCmsContent<CmsGetBlogPostResponse>(
            `query getBlogPost($slug:String!) {
                blog(where:{slug:$slug}) {
                  createdAt
                  slug
                  tags
                  title,
                  overview,
                  category,
                  body {
                    json
                  },
                  createdBy {
                    name
                  }
                }
              }`,
            { slug }
        );

        if (!result) {
            return null;
        }

        return result.data.blog;
    }
    catch (err) {
        console.error('Failed to get blog post', err);
    }

    return null;
}

export async function getBlogMetadata(slug: string): Promise<Metadata> {

    try {

        const result = await fetchCmsContent<CmsBlogMetaResponse>(
            `query getBlogPostMeta($slug:String!) {
                blog(where:{slug:$slug}) {
                    title,
                    overview,
                    category,
                    createdBy {
                        name
                    },
                    tags,
                    metadata {
                        title,
                        description,
                        image {
                            width,
                            height,
                            url
                        }
                    },
                    createdAt,
                    updatedAt
                }
              }`,
            { slug }
        );

        if (!result) {
            return {};
        }

        return {
            title: result.data.blog.metadata.title,
            description: result.data.blog.metadata.description,
            openGraph: {
                images: [result.data.blog.metadata.image?.url],
                title: result.data.blog.metadata.title,
                description: result.data.blog.metadata.description,
                authors: [result.data.blog.createdBy.name],
                type: 'article',
                url: `https://blog.dotnethelp.co.uk/blog/${slug}`,
                tags: result.data.blog.tags,
                modifiedTime: result.data.blog.updatedAt,
                publishedTime: result.data.blog.createdAt,
                locale: 'en_GB'
            },
        }
    }
    catch (err) {
        console.error('Failed to get blog post metadata', err);
    }

    return {};
}

export async function listBlogPosts(): Promise<BlogListItem[]> {
    try {

        const result = await fetchCmsContent<CmsBlogListResponse>(
            `query getBlogPosts {
                blogs(orderBy: createdAt_DESC) {
                  createdAt,
                  slug,
                  tags,
                  title,
                  overview,
                  category,
                  metadata {
                    title,
                    description,
                    image {
                        width,
                        height,
                        url
                    }
                  }
                }
              }`
        );
        if (!result) {
            return [];
        }

        return result.data.blogs;
    }
    catch (err) {
        console.error('Failed to build blog menu', err);
    }

    return [];
}

export async function listBlogPostsByTag(tag: string): Promise<BlogListItem[]> {
    try {

        const result = await fetchCmsContent<CmsBlogListResponse>(
            `query getBlogPostsByTag($tags:[String!])  {
                blogs (where: {tags_contains_all: $tags}, orderBy: createdAt_DESC) {
                    createdAt,
                    slug,
                    tags,
                    title,
                    overview,
                    category,
                    metadata {
                        title,
                        description,
                        image {
                            width,
                            height,
                            url
                        }
                    }
                }
            }`,
            { tags: [tag] }
        );

        if (!result) {
            return [];
        }

        return result.data.blogs;
    }
    catch (err) {
        console.error('Failed to build blog menu', err);
    }

    return [];
}