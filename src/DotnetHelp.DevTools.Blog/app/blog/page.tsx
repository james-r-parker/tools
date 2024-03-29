import { PostOverview } from "@/components/overview";
import Title from "@/components/title";
import { listBlogPosts } from "@/lib/blog";

export default async function Blogs() {
    const pages = await listBlogPosts();

    return <article>
        <Title title="Blog" overview="Blog Posts" />
        <div className="mt-5 grid gap-5 md:grid-cols-2">
            {pages.map((post, index) => <PostOverview key={index} post={post} />)}
        </div>
    </article>
}